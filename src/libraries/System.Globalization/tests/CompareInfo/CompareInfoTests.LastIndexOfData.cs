// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace System.Globalization.Tests
{
    public class CompareInfoLastIndexOfTestsData
    {
        public static CompareInfo s_invariantCompare = CultureInfo.InvariantCulture.CompareInfo;
        private static CompareInfo s_germanCompare = new CultureInfo("de-DE").CompareInfo;
        private static CompareInfo s_hungarianCompare = new CultureInfo("hu-HU").CompareInfo;
        private static CompareInfo s_turkishCompare = new CultureInfo("tr-TR").CompareInfo;
        private static CompareInfo s_slovakCompare = new CultureInfo("sk-SK").CompareInfo;

        public static IEnumerable<object[]> LastIndexOf_TestData()
        {
            bool useNls = PlatformDetection.IsNlsGlobalization;

            // Empty strings
            yield return new object[] { s_invariantCompare, "foo", "", 2, 3, CompareOptions.None, 3, 0 };
            yield return new object[] { s_invariantCompare, "", "", 0, 0, CompareOptions.None, 0, 0 };
            yield return new object[] { s_invariantCompare, "", "a", 0, 0, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "", "", -1, 0, CompareOptions.None, 0, 0 };
            yield return new object[] { s_invariantCompare, "", "a", -1, 0, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "", "", 0, -1, CompareOptions.None, 0, 0 };
            yield return new object[] { s_invariantCompare, "", "a", 0, -1, CompareOptions.None, -1, 0 };

            // Start index = source.Length
            yield return new object[] { s_invariantCompare, "Hello", "l", 5, 5, CompareOptions.None, 3, 1 };
            yield return new object[] { s_invariantCompare, "Hello", "b", 5, 5, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "Hello", "l", 5, 0, CompareOptions.None, -1, 0 };

            yield return new object[] { s_invariantCompare, "Hello", "", 5, 5, CompareOptions.None, 5, 0 };
            yield return new object[] { s_invariantCompare, "Hello", "", 5, 0, CompareOptions.None, 5, 0 };

            // // OrdinalIgnoreCase
            yield return new object[] { s_invariantCompare, "Hello", "l", 4, 5, CompareOptions.OrdinalIgnoreCase, 3, 1 };
            yield return new object[] { s_invariantCompare, "Hello", "L", 4, 5, CompareOptions.OrdinalIgnoreCase, 3, 1 };
            yield return new object[] { s_invariantCompare, "Hello", "h", 4, 5, CompareOptions.OrdinalIgnoreCase, 0, 1 };

            // Long strings
            yield return new object[] { s_invariantCompare, new string('a', 5555) + new string('b', 100), "aaaaaaaaaaaaaaa", 5654, 5655, CompareOptions.None, 5540, 15 };
            yield return new object[] { s_invariantCompare, new string('b', 101) + new string('a', 5555), new string('a', 5000), 5655, 5656, CompareOptions.None, 656, 5000 };
            yield return new object[] { s_invariantCompare, new string('a', 5555), new string('a', 5000) + "b", 5554, 5555, CompareOptions.None, -1, 0 };

            // // Hungarian
            yield return new object[] { s_hungarianCompare, "foobardzsdzs", "rddzs", 11, 12, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, "foobardzsdzs", "rddzs", 11, 12, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "foobardzsdzs", "rddzs", 11, 12, CompareOptions.Ordinal, -1, 0 };

            if (!PlatformDetection.IsHybridGlobalization)
            {
                // Slovak
                yield return new object[] { s_slovakCompare, "ch", "h", 0, 1, CompareOptions.None, -1, 0 };
                // Android has its own ICU, which doesn't work well with slovak
                if (!PlatformDetection.IsAndroid && !PlatformDetection.IsLinuxBionic)
                {
                    yield return new object[] { s_slovakCompare, "hore chodit", "HO", 11, 12, CompareOptions.IgnoreCase, 0, 2 };
                }
                yield return new object[] { s_slovakCompare, "chh", "h", 2, 2, CompareOptions.None, 2, 1 };
            }

            // Turkish
            // Android has its own ICU, which doesn't work well with tr
            if (!PlatformDetection.IsAndroid && !PlatformDetection.IsLinuxBionic)
            {
                yield return new object[] { s_turkishCompare, "Hi", "I", 1, 2, CompareOptions.IgnoreCase, -1, 0 };
                yield return new object[] { s_turkishCompare, "Hi", "\u0130", 1, 2, CompareOptions.IgnoreCase, 1, 1 };
            }
            yield return new object[] { s_turkishCompare, "Hi", "I", 1, 2, CompareOptions.None, -1, 0 };
            yield return new object[] { s_turkishCompare, "Hi", "\u0130", 1, 2, CompareOptions.None, -1, 0 };

            yield return new object[] { s_invariantCompare, "Hi", "I", 1, 2, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "Hi", "I", 1, 2, CompareOptions.IgnoreCase, 1, 1 };
            yield return new object[] { s_invariantCompare, "Hi", "\u0130", 1, 2, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "Hi", "\u0130", 1, 2, CompareOptions.IgnoreCase, -1, 0 };

            // Unicode
            yield return new object[] { s_invariantCompare, "Exhibit \u00C0", "A\u0300", 8, 9, CompareOptions.None, 8, 1 };
            yield return new object[] { s_invariantCompare, "Exhibit \u00C0", "A\u0300", 8, 9, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, "Exhibit \u00C0", "a\u0300", 8, 9, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "Exhibit \u00C0", "a\u0300", 8, 9, CompareOptions.IgnoreCase, 8, 1 };
            yield return new object[] { s_invariantCompare, "Exhibit \u00C0", "a\u0300", 8, 9, CompareOptions.OrdinalIgnoreCase, -1, 0 };
            yield return new object[] { s_invariantCompare, "Exhibit \u00C0", "a\u0300", 8, 9, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, "FooBar", "Foo\u0400Bar", 5, 6, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, "TestFooBA\u0300R", "FooB\u00C0R", 10, 11, CompareOptions.IgnoreNonSpace, 4, 7 };
            yield return new object[] { s_invariantCompare, "o\u0308", "o", 1, 2, CompareOptions.None, -1, 0 };
            if (!PlatformDetection.IsHybridGlobalization)
                yield return new object[] { s_invariantCompare, "\r\n", "\n", 1, 2, CompareOptions.None, 1, 1 };

            // Weightless characters
            // NLS matches weightless characters at the end of the string
            // ICU matches weightless characters at 1 index prior to the end of the string
            yield return new object[] { s_invariantCompare, "", "\u200d", 0, 0, CompareOptions.None, 0, 0 };
            yield return new object[] { s_invariantCompare, "", "\u200d", -1, 0, CompareOptions.None, 0, 0 };
            yield return new object[] { s_invariantCompare, "hello", "\u200d", 4, 5, CompareOptions.IgnoreCase, 5, 0};
            yield return new object[] { s_invariantCompare, "hello", "\0", 4, 5, CompareOptions.None, useNls ? -1 : 5, 0};

            yield return new object[] { s_invariantCompare, "A\u0303", "\u200d", 1, 2, CompareOptions.None, 2, 0};
            yield return new object[] { s_invariantCompare, "A\u0303\u200D", "\u200d", 2, 3, CompareOptions.None, 3, 0};
            yield return new object[] { s_invariantCompare, "\u0001F601", "\u200d", 1, 2, CompareOptions.None, 2, 0}; // \u0001F601 is GRINNING FACE WITH SMILING EYES surrogate character
            yield return new object[] { s_invariantCompare, "AA\u200DA", "\u200d", 3, 4, CompareOptions.None, 4, 0};

            // Ignore symbols
            yield return new object[] { s_invariantCompare, "More Test's", "Tests", 10, 11, CompareOptions.IgnoreSymbols, 5, 6 };
            yield return new object[] { s_invariantCompare, "More Test's", "Tests", 10, 11, CompareOptions.None, -1, 0 };
            yield return new object[] { s_invariantCompare, "cbabababdbaba", "ab", 12, 13, CompareOptions.None, 10, 2 };

            // Platform differences
            if (PlatformDetection.IsNlsGlobalization)
            {
                yield return new object[] { s_hungarianCompare, "foobardzsdzs", "rddzs", 11, 12, CompareOptions.None, 5, 7 };
            }
            else
            {
                yield return new object[] { s_hungarianCompare, "foobardzsdzs", "rddzs", 11, 12, CompareOptions.None, -1, 0 };
            }

            // Inputs where matched length does not equal value string length
            if (!PlatformDetection.IsHybridGlobalization)
            {
                yield return new object[] { s_invariantCompare, "abcdzxyz", "\u01F3", 7, 8, CompareOptions.IgnoreNonSpace, 3, 2 };
                yield return new object[] { s_invariantCompare, "abc\u01F3xyz", "dz", 6, 7, CompareOptions.IgnoreNonSpace, 3, 1 };
                yield return new object[] { s_germanCompare, "abc Strasse Strasse xyz", "stra\u00DFe", 22, 23, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace, 12, 7 };
                yield return new object[] { s_germanCompare, "abc stra\u00DFe stra\u00DFe xyz", "Strasse", 20, 21, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace, 11, 6 };
            }
            yield return new object[] { s_germanCompare, "abc Strasse Strasse xyz", "xtra\u00DFe", 22, 23, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace, -1, 0 };
            yield return new object[] { s_germanCompare, "abc stra\u00DFe stra\u00DFe xyz", "Xtrasse", 20, 21, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace, -1, 0 };
        }

        public static IEnumerable<object[]> LastIndexOf_Aesc_Ligature_TestData()
        {
            bool useNls = PlatformDetection.IsNlsGlobalization;

            // Searches for the ligature \u00C6
            string source = "Is AE or ae the same as \u00C6 or \u00E6?";
            yield return new object[] { s_invariantCompare, source, "AE", 25, 18, CompareOptions.None, useNls ? 24 : -1, useNls ? 1 : 0 };
            yield return new object[] { s_invariantCompare, source, "ae", 25, 18, CompareOptions.None, 9, 2 };
            yield return new object[] { s_invariantCompare, source, '\u00C6', 25, 18, CompareOptions.None, 24, 1 };
            yield return new object[] { s_invariantCompare, source, '\u00E6', 25, 18, CompareOptions.None, useNls ? 9 : -1, useNls ? 2 : 0 };
            yield return new object[] { s_invariantCompare, source, "AE", 25, 18, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, source, "ae", 25, 18, CompareOptions.Ordinal, 9, 2 };
            yield return new object[] { s_invariantCompare, source, '\u00C6', 25, 18, CompareOptions.Ordinal, 24, 1 };
            yield return new object[] { s_invariantCompare, source, '\u00E6', 25, 18, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, source, "AE", 25, 18, CompareOptions.IgnoreCase, useNls ? 24 : 9, useNls ? 1 : 2 };
            yield return new object[] { s_invariantCompare, source, "ae", 25, 18, CompareOptions.IgnoreCase, useNls ? 24 : 9, useNls ? 1 : 2 };
            yield return new object[] { s_invariantCompare, source, '\u00C6', 25, 18, CompareOptions.IgnoreCase, 24, 1 };
            yield return new object[] { s_invariantCompare, source, '\u00E6', 25, 18, CompareOptions.IgnoreCase, 24, 1 };
        }

        public static IEnumerable<object[]> LastIndexOf_U_WithDiaeresis_TestData()
        {
            // Searches for the combining character sequence Latin capital letter U with diaeresis or Latin small letter u with diaeresis.
            string source = "Is \u0055\u0308 or \u0075\u0308 the same as \u00DC or \u00FC?";
            yield return new object[] { s_invariantCompare, source, "U\u0308", 25, 18, CompareOptions.None, 24, 1 };
            yield return new object[] { s_invariantCompare, source, "u\u0308", 25, 18, CompareOptions.None, 9, 2 };
            yield return new object[] { s_invariantCompare, source, '\u00DC', 25, 18, CompareOptions.None, 24, 1 };
            yield return new object[] { s_invariantCompare, source, '\u00FC', 25, 18, CompareOptions.None, 9, 2 };
            yield return new object[] { s_invariantCompare, source, "U\u0308", 25, 18, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, source, "u\u0308", 25, 18, CompareOptions.Ordinal, 9, 2 };
            yield return new object[] { s_invariantCompare, source, '\u00DC', 25, 18, CompareOptions.Ordinal, 24, 1 };
            yield return new object[] { s_invariantCompare, source, '\u00FC', 25, 18, CompareOptions.Ordinal, -1, 0 };
            yield return new object[] { s_invariantCompare, source, "U\u0308", 25, 18, CompareOptions.IgnoreCase, 24, 1 };
            yield return new object[] { s_invariantCompare, source, "u\u0308", 25, 18, CompareOptions.IgnoreCase, 24, 1 };
            yield return new object[] { s_invariantCompare, source, '\u00DC', 25, 18, CompareOptions.IgnoreCase, 24, 1 };
            yield return new object[] { s_invariantCompare, source, '\u00FC', 25, 18, CompareOptions.IgnoreCase, 24, 1 };
        }

        public static void LastIndexOf_Char(CompareInfo compareInfo, string source, char value, int startIndex, int count, CompareOptions options, int expected)
        {
            if (options == CompareOptions.None)
            {
                // Use LastIndexOf(string, char, int, int) or LastIndexOf(string, char)
                if (startIndex + 1 == source.Length && count == source.Length)
                {
                    // Use LastIndexOf(string, char)
                    Assert.Equal(expected, compareInfo.LastIndexOf(source, value));
                }
                // Use LastIndexOf(string, char, int, int)
                Assert.Equal(expected, compareInfo.LastIndexOf(source, value, startIndex, count));
            }
            if (count - startIndex - 1 == 0)
            {
                // Use LastIndexOf(string, char, int, CompareOptions) or LastIndexOf(string, char, CompareOptions)
                if (startIndex == source.Length)
                {
                    // Use LastIndexOf(string, char, CompareOptions)
                    Assert.Equal(expected, compareInfo.LastIndexOf(source, value, options));
                }
                // Use LastIndexOf(string, char, int, CompareOptions)
                Assert.Equal(expected, compareInfo.LastIndexOf(source, value, startIndex, options));
            }
            // Use LastIndexOf(string, char, int, int, CompareOptions)
            Assert.Equal(expected, compareInfo.LastIndexOf(source, value, startIndex, count, options));
        }

        // Attempts to create a Rune from the entirety of a given text buffer.
        public static bool TryCreateRuneFrom(ReadOnlySpan<char> text, out Rune value)
        {
            return Rune.DecodeFromUtf16(text, out value, out int charsConsumed) == OperationStatus.Done
                && charsConsumed == text.Length;
        }
    }
}
