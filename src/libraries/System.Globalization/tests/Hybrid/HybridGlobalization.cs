// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Reflection;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace System.Globalization.Tests
{
    public class HybridGlobalizationTests
    {
        private static CompareInfo s_polishCompare = new CultureInfo("pl-PL").CompareInfo;
        public static IEnumerable<object[]> ToUpper_TestData_Invariant() => InvariantTestData.ToUpper_TestData();
        public static IEnumerable<object[]> ToLower_TestData_Invariant() => InvariantTestData.ToLower_TestData();
        public static IEnumerable<object[]> ToUpper_TestData() => TextInfoMiscTestsData.ToUpper_TestData();
        public static IEnumerable<object[]> ToLower_TestData() => TextInfoMiscTestsData.ToLower_TestData();
        public static IEnumerable<object[]> Compare_TestData() => CompareInfoCompareTestsData.Compare_TestData();
        public static IEnumerable<object[]> Compare_Advanced_TestData() => CompareInfoCompareTestsData.Compare_Advanced_TestData();
        public static IEnumerable<object[]> Compare_Kana_TestData() => CompareInfoCompareTestsData.Compare_Kana_TestData();

        [Theory]
        [MemberData(nameof(ToUpper_TestData_Invariant))]
        public void ToUpperInvariant(string lower, string upper, bool result) // uses ChangeCaseInvariantJS
        {
            Assert.Equal(result, CultureInfo.GetCultureInfo("").TextInfo.ToUpper(lower).Equals(upper, StringComparison.Ordinal));
            Assert.Equal(result, lower.ToUpper().Equals(upper, StringComparison.Ordinal));
        }

        [Theory]
        [MemberData(nameof(ToLower_TestData_Invariant))]
        public void ToLowerInvariant(string upper, string lower, bool result)
        {
            Assert.Equal(result, CultureInfo.GetCultureInfo("").TextInfo.ToLower(upper).Equals(lower, StringComparison.Ordinal));
            Assert.Equal(result, upper.ToLower().Equals(lower, StringComparison.Ordinal));
        }

        [Theory]
        [MemberData(nameof(ToUpper_TestData))]
        public void ToUpper(string name, string str, string expected) // uses ChangeCaseJS
        {
            Assert.Equal(expected, new CultureInfo(name).TextInfo.ToUpper(str));
            if (str.Length == 1)
            {
                Assert.Equal(expected[0], new CultureInfo(name).TextInfo.ToUpper(str[0]));
            }
        }

        [Theory]
        [MemberData(nameof(ToLower_TestData))]
        public void ToLower(string name, string str, string expected)
        {
            Assert.Equal(expected, new CultureInfo(name).TextInfo.ToLower(str));
            if (str.Length == 1)
            {
                Assert.Equal(expected[0], new CultureInfo(name).TextInfo.ToLower(str[0]));
            }
        }

        [Theory]
        [InlineData(CompareOptions.None, "a", "\uFF41",  -1)]
        [InlineData(CompareOptions.None, "a", "á", -1)]
        [InlineData(CompareOptions.None, "a", "A", -1)]
        [InlineData(CompareOptions.None, "á", "A", 1)]
        [InlineData(CompareOptions.IgnoreCase, "a", "á", -1)]
        [InlineData(CompareOptions.IgnoreCase, "a", "A", 0)]
        [InlineData(CompareOptions.IgnoreCase, "á", "A", 1)]
        [InlineData(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace, "a", "á", 0)]
        [InlineData(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace, "a", "A", 0)]
        [InlineData(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace, "á", "A", 0)]
        [InlineData(CompareOptions.IgnoreSymbols, "%a", "a", 0)]
        [InlineData(CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase, "%A", "a", 0)]
        [InlineData(CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace, "%a", "á", 0)]
        [InlineData(CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase, "%A", "á", 0)]
        public void CompareBasic(CompareOptions options, string string1, string string2, int expected)
        {
            Assert.Equal(expected, Math.Sign(String.Compare(string1, string2, CultureInfo.InvariantCulture, options)));
        }

        [Theory]
        [MemberData(nameof(Compare_TestData))]
        [MemberData(nameof(Compare_Kana_TestData))]
        public void Compare(CompareInfo compareInfo, string string1, string string2, CompareOptions options, int expected)
        {
            Compare_Advanced(compareInfo, string1, 0, string1?.Length ?? 0, string2, 0, string2?.Length ?? 0, options, expected);
        }

        [Theory]
        [MemberData(nameof(Compare_Advanced_TestData))]
        public void Compare_Advanced(CompareInfo compareInfo, string string1, int offset1, int length1, string string2, int offset2, int length2, CompareOptions options, int expected)
        {
            if (offset1 + length1 == (string1?.Length ?? 0) && offset2 + length2 == (string2?.Length ?? 0))
            {
                if (offset1 == 0 && offset2 == 0)
                {
                    if (options == CompareOptions.None)
                    {
                        // Use Compare(string, string)
                        Assert.Equal(expected, Math.Sign(compareInfo.Compare(string1, string2)));
                        Assert.Equal(-expected, Math.Sign(compareInfo.Compare(string2, string1)));
                    }
                    // Use Compare(string, string, CompareOptions)
                    Assert.Equal(expected, Math.Sign(compareInfo.Compare(string1, string2, options)));
                    Assert.Equal(-expected, Math.Sign(compareInfo.Compare(string2, string1, options)));
                }
                if (options == CompareOptions.None)
                {
                    // Use Compare(string, int, string, int)
                    Assert.Equal(expected, Math.Sign(compareInfo.Compare(string1, offset1, string2, offset2)));
                    Assert.Equal(-expected, Math.Sign(compareInfo.Compare(string2, offset2, string1, offset1)));
                }
                // Use Compare(string, int, string, int, CompareOptions)
                Assert.Equal(expected, Math.Sign(compareInfo.Compare(string1, offset1, string2, offset2, options)));
                Assert.Equal(-expected, Math.Sign(compareInfo.Compare(string2, offset2, string1, offset1, options)));
            }
            if (options == CompareOptions.None)
            {
                // Use Compare(string, int, int, string, int, int)
                Assert.Equal(expected, Math.Sign(compareInfo.Compare(string1, offset1, length1, string2, offset2, length2)));
                Assert.Equal(-expected, Math.Sign(compareInfo.Compare(string2, offset2, length2, string1, offset1, length1)));
            }
            // Use Compare(string, int, int, string, int, int, CompareOptions)
            Assert.Equal(expected, Math.Sign(compareInfo.Compare(string1, offset1, length1, string2, offset2, length2, options)));
            Assert.Equal(-expected, Math.Sign(compareInfo.Compare(string2, offset2, length2, string1, offset1, length1, options)));

            // Now test the span-based versions - use BoundedMemory to detect buffer overruns
            // We can't run this test for null inputs since they implicitly convert to empty span

            if (string1 != null && string2 != null)
            {
                RunSpanCompareTest(compareInfo, string1.AsSpan(offset1, length1), string2.AsSpan(offset2, length2), options, expected);
            }

            static void RunSpanCompareTest(CompareInfo compareInfo, ReadOnlySpan<char> string1, ReadOnlySpan<char> string2, CompareOptions options, int expected)
            {
                using BoundedMemory<char> string1BoundedMemory = BoundedMemory.AllocateFromExistingData(string1);
                string1BoundedMemory.MakeReadonly();

                using BoundedMemory<char> string2BoundedMemory = BoundedMemory.AllocateFromExistingData(string2);
                string2BoundedMemory.MakeReadonly();

                Assert.Equal(expected, Math.Sign(compareInfo.Compare(string1, string2, options)));
                Assert.Equal(-expected, Math.Sign(compareInfo.Compare(string2, string1, options)));
            }
        }

        [Theory]
        [InlineData(CompareOptions.None, "\u017a", "\u0179", -1)] // ź, Ź
        [InlineData(CompareOptions.OrdinalIgnoreCase, "\u017a", "\u0179", 0)] // ź, Ź
        [InlineData(CompareOptions.Ordinal, "\u017a", "\u0179", 1)] // ź, Ź
        [InlineData(CompareOptions.None, "\u0119", "\u0118", -1)] // ę, Ę
        [InlineData(CompareOptions.OrdinalIgnoreCase, "\u0119", "\u0118", 0)] // ę, Ę
        [InlineData(CompareOptions.Ordinal, "\u0119", "\u0118", 1)] // ę, Ę
        public void CompareOrdinal(CompareOptions options, string string1, string string2, int expected)
        {
            Assert.Equal(expected, Math.Sign(String.Compare(string1, string2, new CultureInfo("pl-PL"), options)));
            Assert.Equal(expected, Math.Sign(s_polishCompare.Compare(string1, string2, options)));
        }

    }
}
