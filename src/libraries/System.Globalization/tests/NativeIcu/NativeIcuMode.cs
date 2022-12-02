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
        [Theory]
        [InlineData("ą", NormalizationForm.FormC)]
        // [InlineData("", NormalizationForm.FormC)]
        // [InlineData("\uFB01", NormalizationForm.FormC)] // fi
        // [InlineData("\uFB01", NormalizationForm.FormD)]
        // [InlineData("\uFB01", NormalizationForm.FormKC)]
        // [InlineData("\uFB01", NormalizationForm.FormKD)]
        // [InlineData("\u1E9b\u0323", NormalizationForm.FormC)]
        // [InlineData("\u1E9b\u0323", NormalizationForm.FormD)]
        // [InlineData("\u1E9b\u0323", NormalizationForm.FormKC)]
        // [InlineData("\u1E9b\u0323", NormalizationForm.FormKD)]
        // [InlineData("\u00C4\u00C7", NormalizationForm.FormC)]
        // [InlineData("\u00C4\u00C7", NormalizationForm.FormD)]
        // [InlineData("A\u0308C\u0327", NormalizationForm.FormC)]
        // [InlineData("A\u0308C\u0327", NormalizationForm.FormD)]
        public void TestNormalization(string s, NormalizationForm form)
        {
            Console.WriteLine($"ILONA: start testing {s}");
            // Assert.True(s.IsNormalized());
            // Assert.True(s.IsNormalized(form));
            // Assert.Equal(s, s.Normalize());
            string normalized = s.Normalize(form);
            Console.WriteLine($"ILONA: normalized = {normalized}");
            // Assert.Equal(s, normalized);
        }

        // [Fact]
        // public static void CulturesLoaded()
        // {
        //     CultureInfo.GetCultures(CultureTypes.AllCultures);
        // }

    }
}
