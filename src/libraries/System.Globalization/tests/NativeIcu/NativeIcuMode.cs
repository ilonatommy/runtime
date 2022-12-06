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
            Assert.Equal(normalized, result);
        }

    }
}
