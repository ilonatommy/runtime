// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace System.Globalization.Tests
{
    public class CompareInfoCompareTests
    {
        public static IEnumerable<object[]> Compare_Kana_TestData() => CompareInfoCompareTestsData.Compare_Kana_TestData();

        public static IEnumerable<object[]> Compare_TestData() => CompareInfoCompareTestsData.Compare_TestData();

        // There is a regression in Windows 190xx version with the Kana comparison. Avoid running this test there.
        public static bool IsNotWindowsKanaRegressedVersion() => !PlatformDetection.IsWindows10Version1903OrGreater ||
                                                              PlatformDetection.IsIcuGlobalization ||
                                                              s_invariantCompare.Compare("\u3060", "\uFF80\uFF9E", CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase) == 0;

        [Fact]
        public void CompareWithUnassignedChars()
        {
            int result = PlatformDetection.IsNlsGlobalization ? 0 : -1;
            Compare(s_invariantCompare, "FooBar", "Foo\uFFFFBar", CompareOptions.None, result);
            Compare(s_invariantCompare, "FooBar", "Foo\uFFFFBar", CompareOptions.IgnoreNonSpace, result);
        }

        [ConditionalTheory(nameof(IsNotWindowsKanaRegressedVersion))]
        [MemberData(nameof(Compare_Kana_TestData))]
        public void CompareWithKana(CompareInfo compareInfo, string string1, string string2, CompareOptions options, int expected)
        {
            Compare_Advanced(compareInfo, string1, 0, string1?.Length ?? 0, string2, 0, string2?.Length ?? 0, options, expected);
        }

        [Theory]
        [MemberData(nameof(Compare_TestData))]
        public void Compare(CompareInfo compareInfo, string string1, string string2, CompareOptions options, int expected)
        {
            Compare_Advanced(compareInfo, string1, 0, string1?.Length ?? 0, string2, 0, string2?.Length ?? 0, options, expected);
        }

        public static IEnumerable<object[]> Compare_Advanced_TestData() => CompareInfoCompareTestsData.Compare_Advanced_TestData();

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

        [Fact]
        public void Compare_Invalid()
        {
            // Compare options are invalid
            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", "Tests", (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", 0, "Tests", 0, (CompareOptions)(-1)));
            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", 0, 2, "Tests", 0, 2, (CompareOptions)(-1)));

            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", "Tests", CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", 0, "Tests", 0, CompareOptions.Ordinal | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", 0, 2, "Tests", 0, 2, CompareOptions.Ordinal | CompareOptions.IgnoreWidth));

            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", "Tests", CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", 0, "Tests", 0, CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));
            AssertExtensions.Throws<ArgumentException>("options", () => s_invariantCompare.Compare("Tests", 0, 2, "Tests", 0, 2, CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreWidth));

            // Offset1 < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset1", () => s_invariantCompare.Compare("Test", -1, "Test", 0));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset1", () => s_invariantCompare.Compare("Test", -1, "Test", 0, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset1", () => s_invariantCompare.Compare("Test", -1, 2, "Test", 0, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset1", () => s_invariantCompare.Compare("Test", -1, 2, "Test", 0, 2, CompareOptions.None));

            // Offset1 > string1.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length1", () => s_invariantCompare.Compare("Test", 5, "Test", 0));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length1", () => s_invariantCompare.Compare("Test", 5, "Test", 0, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string1", () => s_invariantCompare.Compare("Test", 5, 0, "Test", 0, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string1", () => s_invariantCompare.Compare("Test", 5, 0, "Test", 0, 2, CompareOptions.None));

            // Offset2 < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset2", () => s_invariantCompare.Compare("Test", 0, "Test", -1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset2", () => s_invariantCompare.Compare("Test", 0, "Test", -1, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", -1, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", -1, 2, CompareOptions.None));

            // Offset2 > string2.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length2", () => s_invariantCompare.Compare("Test", 0, "Test", 5));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length2", () => s_invariantCompare.Compare("Test", 0, "Test", 5, CompareOptions.None));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 5, 0));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 5, 0, CompareOptions.None));

            // Length1 < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length1", () => s_invariantCompare.Compare("Test", 0, -1, "Test", 0, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length1", () => s_invariantCompare.Compare("Test", 0, -1, "Test", 0, 2, CompareOptions.None));

            // Length1 > string1.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string1", () => s_invariantCompare.Compare("Test", 0, 5, "Test", 0, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string1", () => s_invariantCompare.Compare("Test", 0, 5, "Test", 0, 2, CompareOptions.None));

            // Length2 < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 0, -1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 0, -1, CompareOptions.None));

            // Length2 > string2.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 0, 5));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 0, 5, CompareOptions.None));

            // Offset1 + length1 > string1.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string1", () => s_invariantCompare.Compare("Test", 2, 3, "Test", 0, 2));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string1", () => s_invariantCompare.Compare("Test", 2, 3, "Test", 0, 2, CompareOptions.None));

            // Offset2 + length2 > string2.Length
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 2, 3));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("string2", () => s_invariantCompare.Compare("Test", 0, 2, "Test", 2, 3, CompareOptions.None));
        }

        [Fact]
        public void TestIgnoreKanaAndWidthCases()
        {
            for (char c = '\uFF41'; c <= '\uFF5A'; c++)
            {
                Assert.False(string.Equals(new string(c, 1), new string((char) (c - 0x20), 1), StringComparison.InvariantCulture), $"Expected '{(int)c:x4}' != '{c - 0x20:x4}'");
                Assert.True(string.Equals(new string(c, 1), new string((char) (c - 0x20), 1), StringComparison.InvariantCultureIgnoreCase), $"Expected '{(int)c:x4}' == '{c - 0x20:x4}'");
            }

            // Edge case of the Ignore Width.
            Assert.False(string.Compare("\u3162\u3163", "\uFFDB\uFFDC", CultureInfo.InvariantCulture, CompareOptions.None) == 0, $"Expect '0x3162 0x3163' != '0xFFDB 0xFFDC'");
            Assert.True(string.Compare("\u3162\u3163", "\uFFDB\uFFDC", CultureInfo.InvariantCulture, CompareOptions.IgnoreWidth) == 0, "Expect '0x3162 0x3163' == '0xFFDB 0xFFDC'");

            const char hiraganaStart = '\u3041';
            const char hiraganaEnd = '\u3096';
            const int hiraganaToKatakanaOffset = 0x30a1 - 0x3041;

            for (Char hiraganaChar = hiraganaStart; hiraganaChar <= hiraganaEnd; hiraganaChar++)
            {
                Assert.False(string.Compare(new string(hiraganaChar, 1), new string((char)(hiraganaChar + hiraganaToKatakanaOffset), 1), CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0,
                                            $"Expect '{(int)hiraganaChar:x4}'  != {(int)hiraganaChar + hiraganaToKatakanaOffset:x4} with CompareOptions.IgnoreCase");
                Assert.True(string.Compare(new string(hiraganaChar, 1), new string((char)(hiraganaChar + hiraganaToKatakanaOffset), 1), CultureInfo.InvariantCulture, CompareOptions.IgnoreKanaType) == 0,
                                            $"Expect '{(int)hiraganaChar:x4}'  == {(int)hiraganaChar + hiraganaToKatakanaOffset:x4} with CompareOptions.IgnoreKanaType");
            }
        }

        [Fact]
        public void TestHiraganaAndKatakana()
        {
            const char hiraganaStart = '\u3041';
            const char hiraganaEnd = '\u3096';
            const int hiraganaToKatakanaOffset = 0x30a1 - 0x3041;
            List<char> hiraganaList = new List<char>();
            for (char c = hiraganaStart; c <= hiraganaEnd; c++) // Hiragana
            {
                // TODO: small hiragana/katakana orders
                // https://github.com/dotnet/runtime/issues/54987
                switch (c)
                {
                    case '\u3041':
                    case '\u3043':
                    case '\u3045':
                    case '\u3047':
                    case '\u3049':
                    case '\u3063':
                    case '\u3083':
                    case '\u3085':
                    case '\u3087':
                    case '\u308E':
                    case '\u3095':
                    case '\u3096':
                        break;
                    default:
                        hiraganaList.Add(c);
                        break;
                }
            }
            for (char c = '\u309D'; c <= '\u309E'; c++) // Hiragana iteration mark
            {
                hiraganaList.Add(c);
            }
            CompareOptions[] options = new[] {
                CompareOptions.None,
                CompareOptions.IgnoreCase,
                CompareOptions.IgnoreNonSpace,
                CompareOptions.IgnoreSymbols,
                CompareOptions.IgnoreKanaType,
                CompareOptions.IgnoreWidth,
                CompareOptions.Ordinal,
                CompareOptions.OrdinalIgnoreCase,
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase,
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace,
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
                CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase,
                CompareOptions.IgnoreWidth | CompareOptions.IgnoreNonSpace,
                CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase,
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreNonSpace,
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
            };

            foreach (var option in options)
            {
                for (int i = 0; i < hiraganaList.Count; i++)
                {
                    char hiraganaChar1 = hiraganaList[i];
                    char katakanaChar1 = (char)(hiraganaChar1 + hiraganaToKatakanaOffset);

                    for (int j = i; j < hiraganaList.Count; j++)
                    {
                        char hiraganaChar2 = hiraganaList[j];
                        char katakanaChar2 = (char)(hiraganaChar2 + hiraganaToKatakanaOffset);

                        int hiraganaResult = s_invariantCompare.Compare(new string(hiraganaChar1, 1), new string(hiraganaChar2, 1), option);
                        int katakanaResult = s_invariantCompare.Compare(new string(katakanaChar1, 1), new string(katakanaChar2, 1), option);
                        Assert.True(hiraganaResult == katakanaResult,
                            $"Expect Compare({(int)hiraganaChar1:x4}, {(int)hiraganaChar2:x4}) == Compare({(int)katakanaChar1:x4}, {(int)katakanaChar2:x4}) with CompareOptions.{option}");
                    }
                }
            }
        }
    }
}
