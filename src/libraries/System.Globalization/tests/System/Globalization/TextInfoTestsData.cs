// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace System.Globalization.Tests
{
    public class TextInfoMiscTestsData
    {
        public static IEnumerable<object[]> TextInfo_TestData()
        {
            yield return new object[] { "", 0x7f, 0x4e4, 0x25, 0x2710, 0x1b5, false };
            yield return new object[] { "en-US", 0x409, 0x4e4, 0x25, 0x2710, 0x1b5, false };
            yield return new object[] { "ja-JP", 0x411, 0x3a4, 0x4f42, 0x2711, 0x3a4, false };
            yield return new object[] { "zh-CN", 0x804, 0x3a8, 0x1f4, 0x2718, 0x3a8, false };
            yield return new object[] { "ar-SA", 0x401, 0x4e8, 0x4fc4, 0x2714, 0x2d0, true };
            yield return new object[] { "ko-KR", 0x412, 0x3b5, 0x5161, 0x2713, 0x3b5, false };
            yield return new object[] { "he-IL", 0x40d, 0x4e7, 0x1f4, 0x2715, 0x35e, true };
        }

        public static IEnumerable<object[]> DutchTitleCaseInfo_TestData()
        {
            yield return new object[] { "nl-NL", "IJ IJ IJ IJ", "ij iJ Ij IJ" };
            yield return new object[] { "nl-be", "IJzeren Eigenschappen", "ijzeren eigenschappen" };
            yield return new object[] { "NL-NL", "Lake IJssel", "lake iJssel" };
            yield return new object[] { "NL-BE", "Boba N' IJango Fett PEW PEW", "Boba n' Ijango fett PEW PEW" };
            yield return new object[] { "en-us", "Ijill And Ijack", "ijill and ijack" };
            yield return new object[] { "de-DE", "Ij Ij IJ Ij", "ij ij IJ ij" };
            yield return new object[] { "he-il", "Ijon't Know What Will Happen.", "Ijon't know what Will happen." };
        }

        public static IEnumerable<object[]> CultureName_TestData()
        {
            yield return new object[] { CultureInfo.InvariantCulture.TextInfo, "" };
            yield return new object[] { new CultureInfo("").TextInfo, "" };
            yield return new object[] { new CultureInfo("en-US").TextInfo, "en-US" };
            yield return new object[] { new CultureInfo("fr-FR").TextInfo, "fr-FR" };
            yield return new object[] { new CultureInfo("EN-us").TextInfo, "en-US" };
            yield return new object[] { new CultureInfo("FR-fr").TextInfo, "fr-FR" };
        }

        public static IEnumerable<object[]> IsReadOnly_TestData()
        {
            yield return new object[] { CultureInfo.ReadOnly(new CultureInfo("en-US")).TextInfo, true };
            yield return new object[] { CultureInfo.InvariantCulture.TextInfo, true };
            yield return new object[] { new CultureInfo("").TextInfo, false };
            yield return new object[] { new CultureInfo("en-US").TextInfo, false };
            yield return new object[] { new CultureInfo("fr-FR").TextInfo, false };
        }

        public static IEnumerable<object[]> Equals_TestData()
        {
            yield return new object[] { CultureInfo.InvariantCulture.TextInfo, CultureInfo.InvariantCulture.TextInfo, true };
            yield return new object[] { CultureInfo.InvariantCulture.TextInfo, new CultureInfo("").TextInfo, true };
            yield return new object[] { CultureInfo.InvariantCulture.TextInfo, new CultureInfo("en-US"), false };

            yield return new object[] { new CultureInfo("en-US").TextInfo, new CultureInfo("en-US").TextInfo, true };
            yield return new object[] { new CultureInfo("en-US").TextInfo, new CultureInfo("fr-FR").TextInfo, false };

            yield return new object[] { new CultureInfo("en-US").TextInfo, null, false };
            yield return new object[] { new CultureInfo("en-US").TextInfo, new object(), false };
            yield return new object[] { new CultureInfo("en-US").TextInfo, 123, false };
            yield return new object[] { new CultureInfo("en-US").TextInfo, "en-US", false };

        }

        private static readonly string [] s_cultureNames = new string[] { "en-US", "fr", "fr-FR" };

        // ToLower_TestData_netcore has the data which is specific to netcore framework
        public static IEnumerable<object[]> ToLower_TestData_netcore()
        {
            foreach (string cultureName in s_cultureNames)
            {
                // DESERT CAPITAL LETTER LONG I has a lower case variant (but not on Windows 7).
                yield return new object[] { cultureName, "\U00010400", PlatformDetection.IsWindows7 ? "\U00010400" : "\U00010428" };
            }

            if (!PlatformDetection.IsNlsGlobalization)
            {
                yield return new object[] { "", "\U00010400", PlatformDetection.IsWindows7 ? "\U00010400" : "\U00010428" };
            }
        }

        public static IEnumerable<string> GetTestLocales()
        {
            yield return "tr";
            yield return "tr-TR";

            if (PlatformDetection.IsNotUsingLimitedCultures)
            {
                // Mobile / Browser ICU doesn't contain these locales
                yield return "az";
                yield return "az-Latn-AZ";
            }
        }

        public static IEnumerable<object[]> ToLower_TestData()
        {
            foreach (string cultureName in s_cultureNames)
            {
                yield return new object[] { cultureName, "", "" };

                yield return new object[] { cultureName, "A", "a" };
                yield return new object[] { cultureName, "a", "a" };
                yield return new object[] { cultureName, "ABC", "abc" };
                yield return new object[] { cultureName, "abc", "abc" };

                yield return new object[] { cultureName, "1", "1" };
                yield return new object[] { cultureName, "123", "123" };
                yield return new object[] { cultureName, "!", "!" };

                yield return new object[] { cultureName, "HELLOWOR!LD123", "hellowor!ld123" };
                yield return new object[] { cultureName, "HelloWor!ld123", "hellowor!ld123" };
                yield return new object[] { cultureName, "Hello\n\0World\u0009!", "hello\n\0world\t!" };

                yield return new object[] { cultureName, "THIS IS A LONGER TEST CASE", "this is a longer test case" };
                yield return new object[] { cultureName, "this Is A LONGER mIXEd casE test case", "this is a longer mixed case test case" };

                yield return new object[] { cultureName, "THIS \t hAs \t SOMe \t tabs", "this \t has \t some \t tabs" };
                yield return new object[] { cultureName, "EMBEDDED\0NuLL\0Byte\0", "embedded\0null\0byte\0" };

                // LATIN CAPITAL LETTER O WITH ACUTE, which has a lower case variant.
                yield return new object[] { cultureName, "\u00D3", "\u00F3" };

                // SNOWMAN, which does not have a lower case variant.
                yield return new object[] { cultureName, "\u2603", "\u2603" };

                // RAINBOW (outside the BMP and does not case)
                yield return new object[] { cultureName, "\U0001F308", "\U0001F308" };

                // Unicode defines some codepoints which expand into multiple codepoints
                // when cased (see SpecialCasing.txt from UNIDATA for some examples). We have never done
                // these sorts of expansions, since it would cause string lengths to change when cased,
                // which is non-intuitive. In addition, there are some context sensitive mappings which
                // we also don't preform.
                // Greek Capital Letter Sigma (does not to case to U+03C2 with "final sigma" rule).
                yield return new object[] { cultureName, "\u03A3", "\u03C3" };
            }

            foreach (string cultureName in GetTestLocales())
            {
                // Android has its own ICU, which doesn't work well with tr
                if (!PlatformDetection.IsAndroid && !PlatformDetection.IsLinuxBionic)
                {
                    yield return new object[] { cultureName, "I", "\u0131" };
                    yield return new object[] { cultureName, "HI!", "h\u0131!" };
                    yield return new object[] { cultureName, "HI\n\0H\u0130\t!", "h\u0131\n\0hi\u0009!" };
                }
                yield return new object[] { cultureName, "\u0130", "i" };
                yield return new object[] { cultureName, "i", "i" };

            }

            // ICU has special tailoring for the en-US-POSIX locale which treats "i" and "I" as different letters
            // instead of two letters with a case difference during collation.  Make sure this doesn't confuse our
            // casing implementation, which uses collation to understand if we need to do Turkish casing or not.
            if (!PlatformDetection.IsWindows && PlatformDetection.IsNotBrowser)
            {
                yield return new object[] { "en-US-POSIX", "I", "i" };
            }
        }

        // ToUpper_TestData_netcore has the data which is specific to netcore framework
        public static IEnumerable<object[]> ToUpper_TestData_netcore()
        {
            foreach (string cultureName in s_cultureNames)
            {
                // DESERT SMALL LETTER LONG I has an upper case variant (but not on Windows 7).
                yield return new object[] { cultureName, "\U00010428", PlatformDetection.IsWindows7 ? "\U00010428" : "\U00010400" };
            }
        }

        public static IEnumerable<object[]> ToUpper_TestData()
        {
            foreach (string cultureName in s_cultureNames)
            {
                yield return new object[] { cultureName, "", "" };

                yield return new object[] { cultureName, "a", "A" };
                yield return new object[] { cultureName, "abc", "ABC" };
                yield return new object[] { cultureName, "A", "A" };
                yield return new object[] { cultureName, "ABC", "ABC" };

                yield return new object[] { cultureName, "1", "1" };
                yield return new object[] { cultureName, "123", "123" };
                yield return new object[] { cultureName, "!", "!" };

                yield return new object[] { cultureName, "HelloWor!ld123", "HELLOWOR!LD123" };
                yield return new object[] { cultureName, "HELLOWOR!LD123", "HELLOWOR!LD123" };
                yield return new object[] { cultureName, "Hello\n\0World\u0009!", "HELLO\n\0WORLD\t!" };

                yield return new object[] { cultureName, "this is a longer test case", "THIS IS A LONGER TEST CASE" };
                yield return new object[] { cultureName, "this Is A LONGER mIXEd casE test case", "THIS IS A LONGER MIXED CASE TEST CASE" };
                yield return new object[] { cultureName, "this \t HaS \t somE \t TABS", "THIS \t HAS \t SOME \t TABS" };

                yield return new object[] { cultureName, "embedded\0NuLL\0Byte\0", "EMBEDDED\0NULL\0BYTE\0" };

                // LATIN SMALL LETTER O WITH ACUTE, which has an upper case variant.
                yield return new object[] { cultureName, "\u00F3", "\u00D3" };

                // SNOWMAN, which does not have an upper case variant.
                yield return new object[] { cultureName, "\u2603", "\u2603" };

                // RAINBOW (outside the BMP and does not case)
                yield return new object[] { cultureName, "\U0001F308", "\U0001F308" };

                // Unicode defines some codepoints which expand into multiple codepoints
                // when cased (see SpecialCasing.txt from UNIDATA for some examples). We have never done
                // these sorts of expansions, since it would cause string lengths to change when cased,
                // which is non-intuitive. In addition, there are some context sensitive mappings which
                // we also don't preform.
                // es-zed does not case to SS when uppercased.
                yield return new object[] { cultureName, "\u00DF", "\u00DF" };

                // Ligatures do not expand when cased.
                yield return new object[] { cultureName, "\uFB00", "\uFB00" };

                // Precomposed character with no uppercase variant, we don't want to "decompose" this
                // as part of casing.
                yield return new object[] { cultureName, "\u0149", "\u0149" };
            }

            // Turkish i
            foreach (string cultureName in GetTestLocales())
            {
                // Android has its own ICU, which doesn't work well with tr
                if (!PlatformDetection.IsAndroid && !PlatformDetection.IsLinuxBionic)
                {
                    yield return new object[] { cultureName, "i", "\u0130" };
                    yield return new object[] { cultureName, "H\u0131\n\0Hi\u0009!", "HI\n\0H\u0130\t!" };
                }
                yield return new object[] { cultureName, "\u0130", "\u0130" };
                yield return new object[] { cultureName, "\u0131", "I" };
                yield return new object[] { cultureName, "I", "I" };
            }

            // ICU has special tailoring for the en-US-POSIX locale which treats "i" and "I" as different letters
            // instead of two letters with a case difference during collation.  Make sure this doesn't confuse our
            // casing implementation, which uses collation to understand if we need to do Turkish casing or not.
            if (!PlatformDetection.IsWindows && PlatformDetection.IsNotBrowser)
            {
                yield return new object[] { "en-US-POSIX", "i", "I" };
            }
        }
    }
}
