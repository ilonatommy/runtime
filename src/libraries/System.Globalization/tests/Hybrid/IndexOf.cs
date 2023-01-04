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
    public class IndexOfTests
    {
        public static IEnumerable<object[]> IndexOf_TestData() => CompareInfoIndexOfTestsData.IndexOf_TestData();
        public static IEnumerable<object[]> IndexOf_Aesc_Ligature_TestData() => CompareInfoIndexOfTestsData.IndexOf_Aesc_Ligature_TestData();
        public static IEnumerable<object[]> IndexOf_U_WithDiaeresis_TestData() => CompareInfoIndexOfTestsData.IndexOf_U_WithDiaeresis_TestData();

        [Theory]
        [MemberData(nameof(IndexOf_TestData))]
        [MemberData(nameof(IndexOf_Aesc_Ligature_TestData))]
        [MemberData(nameof(IndexOf_U_WithDiaeresis_TestData))]
        public void IndexOf_String(CompareInfo compareInfo, string source, string value, int startIndex, int count, CompareOptions options, int expected, int expectedMatchLength)
        {
            if (value.Length == 1)
            {
                CompareInfoIndexOfTestsData.IndexOf_Char(compareInfo, source, value[0], startIndex, count, options, expected);
            }
            if (options == CompareOptions.None)
            {
                // Use IndexOf(string, string, int, int) or IndexOf(string, string)
                if (startIndex == 0 && count == source.Length)
                {
                    // Use IndexOf(string, string)
                    Assert.Equal(expected, compareInfo.IndexOf(source, value));
                }
                // Use IndexOf(string, string, int, int)
                Assert.Equal(expected, compareInfo.IndexOf(source, value, startIndex, count));
            }
            if (startIndex + count == source.Length)
            {
                // Use IndexOf(string, string, int, CompareOptions) or IndexOf(string, string, CompareOptions)
                if (startIndex == 0)
                {
                    // Use IndexOf(string, string, CompareOptions)
                    Assert.Equal(expected, compareInfo.IndexOf(source, value, options));
                }
                // Use IndexOf(string, string, int, CompareOptions)
                Assert.Equal(expected, compareInfo.IndexOf(source, value, startIndex, options));
            }
            // Use IndexOf(string, string, int, int, CompareOptions)
            Assert.Equal(expected, compareInfo.IndexOf(source, value, startIndex, count, options));

            if ((compareInfo == CompareInfoIndexOfTestsData.s_invariantCompare) && ((options == CompareOptions.None) || (options == CompareOptions.IgnoreCase)))
            {
                StringComparison stringComparison = (options == CompareOptions.IgnoreCase) ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                // Use int string.IndexOf(string, StringComparison)
                Assert.Equal(expected, source.IndexOf(value, startIndex, count, stringComparison));
                // Use int MemoryExtensions.IndexOf(this ReadOnlySpan<char>, ReadOnlySpan<char>, StringComparison)
                Assert.Equal((expected == -1) ? -1 : (expected - startIndex), source.AsSpan(startIndex, count).IndexOf(value.AsSpan(), stringComparison));
            }

            // Now test the span-based versions - use BoundedMemory to detect buffer overruns

            RunSpanIndexOfTest(compareInfo, source.AsSpan(startIndex, count), value, options, (expected < 0) ? expected : expected - startIndex, expectedMatchLength);

            static void RunSpanIndexOfTest(CompareInfo compareInfo, ReadOnlySpan<char> source, ReadOnlySpan<char> value, CompareOptions options, int expected, int expectedMatchLength)
            {
                using BoundedMemory<char> sourceBoundedMemory = BoundedMemory.AllocateFromExistingData(source);
                sourceBoundedMemory.MakeReadonly();

                using BoundedMemory<char> valueBoundedMemory = BoundedMemory.AllocateFromExistingData(value);
                valueBoundedMemory.MakeReadonly();

                Assert.Equal(expected, compareInfo.IndexOf(sourceBoundedMemory.Span, valueBoundedMemory.Span, options));
                Assert.Equal(expected, compareInfo.IndexOf(sourceBoundedMemory.Span, valueBoundedMemory.Span, options, out int actualMatchLength));
                Assert.Equal(expectedMatchLength, actualMatchLength);

                if (CompareInfoIndexOfTestsData.TryCreateRuneFrom(value, out Rune rune))
                {
                    Assert.Equal(expected, compareInfo.IndexOf(sourceBoundedMemory.Span, rune, options)); // try the Rune-based version
                }
            }
        }
    }
}
