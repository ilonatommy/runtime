// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace System.Globalization.Tests
{
    public class CompareInfoLastIndexOfTests
    {
        private static CompareInfo CompareInfoLastIndexOfTestsData.s_invariantCompare = CultureInfo.InvariantCulture.CompareInfo;
        public static IEnumerable<object[]> LastIndexOf_TestData() => CompareInfoLastIndexOfTestsData.LastIndexOf_TestData();
        public static IEnumerable<object[]> LastIndexOf_U_WithDiaeresis_TestData() => CompareInfoLastIndexOfTestsData.LastIndexOf_U_WithDiaeresis_TestData();
        public static IEnumerable<object[]> LastIndexOf_Aesc_Ligature_TestData() => CompareInfoLastIndexOfTestsData.LastIndexOf_Aesc_Ligature_TestData();

        [Theory]
        [MemberData(nameof(LastIndexOf_TestData))]
        [MemberData(nameof(LastIndexOf_U_WithDiaeresis_TestData))]
        public void LastIndexOf_String(CompareInfo compareInfo, string source, string value, int startIndex, int count, CompareOptions options, int expected, int expectedMatchLength)
        {
            if (value.Length == 1)
            {
                CompareInfoLastIndexOfTestsData.LastIndexOf_Char(compareInfo, source, value[0], startIndex, count, options, expected);
            }
            if (options == CompareOptions.None)
            {
                // Use LastIndexOf(string, string, int, int) or LastIndexOf(string, string)
                if (startIndex + 1 == source.Length && count == source.Length)
                {
                    // Use LastIndexOf(string, string)
                    Assert.Equal(expected, compareInfo.LastIndexOf(source, value));
                }
                // Use LastIndexOf(string, string, int, int)
                Assert.Equal(expected, compareInfo.LastIndexOf(source, value, startIndex, count));
            }
            if (count - startIndex - 1 == 0)
            {
                // Use LastIndexOf(string, string, int, CompareOptions) or LastIndexOf(string, string, CompareOptions)
                if (startIndex == source.Length)
                {
                    // Use LastIndexOf(string, string, CompareOptions)
                    Assert.Equal(expected, compareInfo.LastIndexOf(source, value, options));
                }
                // Use LastIndexOf(string, string, int, CompareOptions)
                Assert.Equal(expected, compareInfo.LastIndexOf(source, value, startIndex, options));
            }
            // Use LastIndexOf(string, string, int, int, CompareOptions)
            Assert.Equal(expected, compareInfo.LastIndexOf(source, value, startIndex, count, options));

            // Fixup offsets so that we can call the span-based APIs.

            ReadOnlySpan<char> sourceSpan;
            int adjustmentFactor; // number of chars to add to retured index from span-based APIs

            if (startIndex == source.Length - 1 && count == source.Length)
            {
                // This idiom means "read the whole span"
                sourceSpan = source;
                adjustmentFactor = 0;
            }
            else if (startIndex == source.Length)
            {
                // Account for possible off-by-one at the call site
                sourceSpan = source.AsSpan()[^(Math.Max(0, count - 1))..];
                adjustmentFactor = source.Length - sourceSpan.Length;
            }
            else
            {
                // Bump 'startIndex' by 1, then go back 'count' chars
                sourceSpan = source.AsSpan()[..(startIndex + 1)][^count..];
                adjustmentFactor = startIndex + 1 - count;
            }

            if (expected < 0) { adjustmentFactor = 0; } // don't modify "not found" (-1) return values

            if ((compareInfo == CompareInfoLastIndexOfTestsData.s_invariantCompare) && ((options == CompareOptions.None) || (options == CompareOptions.IgnoreCase)))
            {
                StringComparison stringComparison = (options == CompareOptions.IgnoreCase) ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

                // Use int string.LastIndexOf(string, StringComparison)
                Assert.Equal(expected, source.LastIndexOf(value, startIndex, count, stringComparison));

                // Use int MemoryExtensions.LastIndexOf(this ReadOnlySpan<char>, ReadOnlySpan<char>, StringComparison)
                Assert.Equal(expected - adjustmentFactor, sourceSpan.LastIndexOf(value.AsSpan(), stringComparison));
            }

            // Now test the span-based versions - use BoundedMemory to detect buffer overruns

            RunSpanLastIndexOfTest(compareInfo, sourceSpan, value, options, expected - adjustmentFactor, expectedMatchLength);

            static void RunSpanLastIndexOfTest(CompareInfo compareInfo, ReadOnlySpan<char> source, ReadOnlySpan<char> value, CompareOptions options, int expected, int expectedMatchLength)
            {
                using BoundedMemory<char> sourceBoundedMemory = BoundedMemory.AllocateFromExistingData(source);
                sourceBoundedMemory.MakeReadonly();

                using BoundedMemory<char> valueBoundedMemory = BoundedMemory.AllocateFromExistingData(value);
                valueBoundedMemory.MakeReadonly();

                Assert.Equal(expected, compareInfo.LastIndexOf(sourceBoundedMemory.Span, valueBoundedMemory.Span, options));
                Assert.Equal(expected, compareInfo.LastIndexOf(sourceBoundedMemory.Span, valueBoundedMemory.Span, options, out int actualMatchLength));
                Assert.Equal(expectedMatchLength, actualMatchLength);

                if (CompareInfoLastIndexOfTestsData.TryCreateRuneFrom(value, out Rune rune))
                {
                    Assert.Equal(expected, compareInfo.LastIndexOf(sourceBoundedMemory.Span, rune, options)); // try the Rune-based version
                }
            }
        }

        [Theory]
        [MemberData(nameof(LastIndexOf_Aesc_Ligature_TestData))]
        public void LastIndexOf_Aesc_Ligature(CompareInfo compareInfo, string source, string value, int startIndex, int count, CompareOptions options, int expected, int expectedMatchLength)
        {
            LastIndexOf_String(compareInfo, source, value, startIndex, count, options, expected, expectedMatchLength);
        }

        [Fact]
        public void LastIndexOf_UnassignedUnicode()
        {
            bool useNls = PlatformDetection.IsNlsGlobalization;
            int expectedMatchLength = (useNls) ? 6 : 0;
            LastIndexOf_String(CompareInfoLastIndexOfTestsData.s_invariantCompare, "FooBar", "Foo\uFFFFBar", 5, 6, CompareOptions.None, useNls ? 0 : -1, expectedMatchLength);
            LastIndexOf_String(CompareInfoLastIndexOfTestsData.s_invariantCompare, "~FooBar", "Foo\uFFFFBar", 6, 7, CompareOptions.IgnoreNonSpace, useNls ? 1 : -1, expectedMatchLength);
        }

        [Fact]
        public void LastIndexOf_Invalid()
        {
            // Source is null
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, "a"));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, "a", CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, "a", 0, 0));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, "a", 0, CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, "a", 0, 0, CompareOptions.None));

            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, 'a'));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, 'a', CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, 'a', 0, 0));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, 'a', 0, CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, 'a', 0, 0, CompareOptions.None));

            // Value is null
            AssertExtensions.Throws<ArgumentNullException>("value", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("", null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("", null, CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("value", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("", null, 0, 0));
            AssertExtensions.Throws<ArgumentNullException>("value", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("", null, 0, CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("value", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("", null, 0, 0, CompareOptions.None));

            // Source and value are null
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, null));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, null, CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, null, 0, 0));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, null, 0, CompareOptions.None));
            AssertExtensions.Throws<ArgumentNullException>("source", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf(null, null, 0, 0, CompareOptions.None));

            // Options are invalid
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", CompareOptions.StringSort));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, CompareOptions.StringSort));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, 1, CompareOptions.StringSort));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', CompareOptions.StringSort));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, CompareOptions.StringSort));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, 1, CompareOptions.StringSort));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), CompareOptions.StringSort));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), CompareOptions.StringSort, out int matchLength));

            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, 1, CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, 1, CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), CompareOptions.Ordinal | CompareOptions.IgnoreWidth, out int matchLength));

            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, 1, CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, 1, CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth, out int matchLength));

            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, 1, (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, 1, (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), (CompareOptions)(-1), out int matchLength));

            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", (CompareOptions)0x11111111));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, (CompareOptions)0x11111111));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "Tests", 0, 1, (CompareOptions)0x11111111));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', (CompareOptions)0x11111111));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, (CompareOptions)0x11111111));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", 'a', 0, 1, (CompareOptions)0x11111111));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), (CompareOptions)0x11111111));
            AssertExtensions.Throws<ArgumentException>("options", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test's", "a".AsSpan(), (CompareOptions)0x11111111, out int matchLength));

            // StartIndex < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", -1, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", -1, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", -1, 2, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', -1, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', -1, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', -1, 2, CompareOptions.None));

            // StartIndex >= source.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 5, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 5, 0));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 5, 0, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 5, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 5, 0));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("startIndex", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 5, 0, CompareOptions.None));

            // Count < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 0, -1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 0, -1, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 0, -1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 0, -1, CompareOptions.None));

            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 4, -1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 4, -1, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 4, -1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 4, -1, CompareOptions.None));

            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "", 4, -1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "", 4, -1, CompareOptions.None));

            // Count > source.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 0, 5));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 0, 5, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 0, 5));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 0, 5, CompareOptions.None));

            // StartIndex + count > source.Length + 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 3, 5));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "Test", 3, 5, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 3, 5));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'a', 3, 5, CompareOptions.None));

            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "s", 4, 6));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "s", 4, 7, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 's', 4, 6));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 's', 4, 7, CompareOptions.None));

            // Count > StartIndex + 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "e", 1, 3));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", "e", 1, 3, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'e', 1, 3));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => CompareInfoLastIndexOfTestsData.s_invariantCompare.LastIndexOf("Test", 'e', 1, 3, CompareOptions.None));
        }
    }
}
