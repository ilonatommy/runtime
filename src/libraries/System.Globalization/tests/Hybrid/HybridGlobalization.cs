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
        public static IEnumerable<object[]> ToUpper_TestData_Invariant() => InvariantTestData.ToUpper_TestData();
        public static IEnumerable<object[]> ToLower_TestData_Invariant() => InvariantTestData.ToLower_TestData();
        public static IEnumerable<object[]> ToUpper_TestData() => TextInfoMiscTestsData.ToUpper_TestData();
        public static IEnumerable<object[]> ToLower_TestData() => TextInfoMiscTestsData.ToLower_TestData();

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
        // this one will keep failing for tr culture (str: "i", expected: "İ") till CompareInfo.Compare(CompareOptions.IgnoreCase) gets implemented
        // it is used in PopulateIsAsciiCasingSameAsInvariant and if icudt_wasm.dat is loaded, it evaluates to true and treats Turkish i like ASCII
        // which is incorrect and results in bypassing Web API function
        // after implementation it will evaluate to false (currently we can force false evaluation by loading not reduced icudt.dat)
        public void ToUpper(string name, string str, string expected)
        {
            Assert.Equal(expected, new CultureInfo(name).TextInfo.ToUpper(str));
            if (str.Length == 1)
            {
                Assert.Equal(expected[0], new CultureInfo(name).TextInfo.ToUpper(str[0]));
            }
        }

        [Theory]
        [MemberData(nameof(ToLower_TestData))]
        // same story here, temporarily all Turkish signs are treated as ASCII
        public void ToLower(string name, string str, string expected)
        {
            Assert.Equal(expected, new CultureInfo(name).TextInfo.ToLower(str));
            if (str.Length == 1)
            {
                Assert.Equal(expected[0], new CultureInfo(name).TextInfo.ToLower(str[0]));
            }
        }
    }
}
