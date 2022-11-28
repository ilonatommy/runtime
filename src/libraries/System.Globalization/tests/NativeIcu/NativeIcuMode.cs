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
        private static bool PredefinedCulturesOnlyIsDisabled { get; } = !PredefinedCulturesOnly();
        private static bool PredefinedCulturesOnly()
        {
            bool ret;

            try
            {
                ret = (bool) typeof(object).Assembly.GetType("System.Globalization.GlobalizationMode").GetProperty("PredefinedCulturesOnly", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        private static readonly string[] s_cultureNames = new string[] { "en-US", "ja-JP", "fr-FR", "tr-TR", "" };

        [ConditionalFact(nameof(PredefinedCulturesOnlyIsDisabled))]
        public static void CulturesLoaded()
        {
            Console.WriteLine($"TEST");
            foreach (var a in CultureInfo.GetCultures(CultureTypes.AllCultures))
                Console.WriteLine($"TEST {a}");
        }

    }
}
