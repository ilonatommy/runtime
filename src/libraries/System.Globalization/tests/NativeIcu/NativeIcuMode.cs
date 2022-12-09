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
    public class NativeIcuModeTests
    {
        // Mobile / Browser ICU doesn't support FormKC and FormKD

        // for more complex tests see and adapt: NormalizationAll.cs
        [Theory]
        [InlineData("", "", NormalizationForm.FormC)]
        [InlineData("\uFB01", "\uFB01", NormalizationForm.FormC)]
        [InlineData("\uFB01", "\uFB01", NormalizationForm.FormD)]
        [InlineData("\u1E9b\u0323", "\u1E9b\u0323", NormalizationForm.FormC)]
        [InlineData("\u1E9b\u0323", "\u017F\u0323\u0307", NormalizationForm.FormD)]
        [InlineData("\u00C4\u00C7", "\u00C4\u00C7", NormalizationForm.FormC)]
        [InlineData("\u00C4\u00C7", "A\u0308C\u0327", NormalizationForm.FormD)]
        [InlineData("A\u0308C\u0327", "\u00C4\u00C7", NormalizationForm.FormC)]
        [InlineData("A\u0308C\u0327", "A\u0308C\u0327", NormalizationForm.FormD)]
        public void TestNormalization(string s, string normalized, NormalizationForm form)
        {
            string result = s.Normalize(form);
            Assert.True(result.IsNormalized(form));
            Assert.Equal(normalized, result);
        }

        [Theory]
        [MemberData(nameof(ToUpper_TestData))]
        public void ToUpper(string name, string str, string expected)
        {
            var culture = new CultureInfo(name);
            var result = culture.TextInfo.ToUpper(str);
            if (str != result){
                Console.WriteLine($"culture = {culture.DisplayName}, str = {str} result = {result}; expected = {expected}");
                Assert.Equal(str, result);
            }
        }

        public static IEnumerable<string> GetTestLocales()
        {
            yield return "tr";
            yield return "tr-TR";
        }

        private static readonly string [] s_cultureNames = new string[] { "en-US", "fr", "fr-FR" };

        public static IEnumerable<object[]> ToUpper_TestData()
        {
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
        }

    }
}
