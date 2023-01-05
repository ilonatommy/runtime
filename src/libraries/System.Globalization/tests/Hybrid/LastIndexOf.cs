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
    public class LastIndexOfTests
    {
        public static IEnumerable<object[]> LastIndexOf_TestData() => CompareInfoLastIndexOfTestsData.LastIndexOf_TestData();
        public static IEnumerable<object[]> LastIndexOf_U_WithDiaeresis_TestData() => CompareInfoLastIndexOfTestsData.LastIndexOf_U_WithDiaeresis_TestData();
        public static IEnumerable<object[]> LastIndexOf_Aesc_Ligature_TestData() => CompareInfoLastIndexOfTestsData.LastIndexOf_Aesc_Ligature_TestData();

        [Theory]
        [MemberData(nameof(LastIndexOf_TestData))]
        [MemberData(nameof(LastIndexOf_U_WithDiaeresis_TestData))]
        [MemberData(nameof(LastIndexOf_Aesc_Ligature_TestData))]
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
    }
}
