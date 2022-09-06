// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Globalization;

#nullable enable

namespace Wasm.Build.Tests
{
    public class ICUShardingTests : BuildTestBase
    {
        public ICUShardingTests(ITestOutputHelper output, SharedBuildPerTestClassFixture buildContext)
            : base(output, buildContext)
        {
        }

        public static IEnumerable<object?[]> ICUShardingByCulture_EFIGS_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    // declaring "de" we make "EFIGS" file to be uploaded, so all
                    // cultures from EFIGS should be accessible as well
                    new object?[] { new string[] { "de" }, "\"de_IT\", \"en_DE\", \"fr_BE\""},
                    new object?[] { new string[] { "de", "en" }, "\"de_IT\", \"en_GI\", \"fr_BE\""},
                    new object?[] { new string[] { "fr" }, "\"en_NZ\", \"es_ES\", \"fr_BE\""}
                    )
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        public static IEnumerable<object?[]> ICUShardingByCulture_CJK_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    new object?[] { new string[] { "zh" }, "\"zh_HK\", \"en_DE\", \"ko_KR\""},
                    new object?[] { new string[] { "en", "ko" }, "\"ja_JP\", \"ko_KR\", \"zh_SG\""},
                    new object?[] { new string[] { "ja" }, "\"en_US\", \"ja_JP\", \"ko_KR\""})
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        public static IEnumerable<object?[]> ICUShardingByCulture_no_CJK_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    new object?[] { new string[] { "pl" }, "\"pl_PL\", \"en_MP\", \"da_DK\""},
                    new object?[] { new string[] { "hr", "en" }, "\"hr_BA\", \"mr_IN\", \"fi_FI\""},
                    new object?[] { new string[] { "cs" }, "\"cs_CZ\", \"da_DK\", \"vi_VN\""})
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        public static IEnumerable<object?[]> ICUShardingByCulture_EFIG_CJK_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    // loads full_full
                    new object?[] { new string[] { "fr", "ja" }, "\"fr_CH\", \"ja_JP\", \"zh_HK\""},
                    new object?[] { new string[] { "es", "zh" }, "\"es_419\", \"zh_HK\", \"de_LI\""},
                    new object?[] { new string[] { "en", "ko", "it" }, "\"it_IT\", \"ko_KR\", \"en_TV\""})
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        // [Theory]
        // [MemberData(nameof(ICUShardingByCulture_EFIGS_Positive), parameters: new object[] { /*aot*/ false, RunHost.All })]
        // [MemberData(nameof(ICUShardingByCulture_EFIGS_Positive), parameters: new object[] { /*aot*/ true, RunHost.All })]
        // [MemberData(nameof(ICUShardingByCulture_CJK_Positive), parameters: new object[] { /*aot*/ false, RunHost.NodeJS })] // for Chrome fails
        // [MemberData(nameof(ICUShardingByCulture_CJK_Positive), parameters: new object[] { /*aot*/ true, RunHost.NodeJS })]
        // [MemberData(nameof(ICUShardingByCulture_no_CJK_Positive), parameters: new object[] { /*aot*/ false, RunHost.All })]
        // [MemberData(nameof(ICUShardingByCulture_no_CJK_Positive), parameters: new object[] { /*aot*/ true, RunHost.All })]
        // [MemberData(nameof(ICUShardingByCulture_EFIG_CJK_Positive), parameters: new object[] { /*aot*/ false, RunHost.NodeJS })] // for Chrome fails
        // [MemberData(nameof(ICUShardingByCulture_EFIG_CJK_Positive), parameters: new object[] { /*aot*/ true, RunHost.NodeJS })]
        // public void ShardingTestsPositive(BuildArgs buildArgs, string[] declaredIcuCultures, string testedCultures, RunHost host, string id)
        //     => TestICUShardingByCulture(buildArgs, declaredIcuCultures, testedCultures, false, host, id,
        //                                     extraProperties: "<WasmBuildNative>true</WasmBuildNative>",
        //                                     dotnetWasmFromRuntimePack: false);

        private void TestICUShardingByCulture(BuildArgs buildArgs,
                             string[] declaredIcuCultures,
                             string testedCulturesStr,
                             bool? invariantGlobalization,
                             RunHost host,
                             string id,
                             string extraProperties="",
                             bool? dotnetWasmFromRuntimePack=null,
                             bool expectFailure=false)
        {
            string projectName = $"sharding_{string.Join("-", declaredIcuCultures)}";
            if (invariantGlobalization != null)
                extraProperties = $"{extraProperties}<InvariantGlobalization>{invariantGlobalization}</InvariantGlobalization>";

            string extraItems = "";
            foreach (var culture in declaredIcuCultures)
                extraItems = $"{extraItems}<WasmIcuCultures Include=\"{culture}\"/>";

            buildArgs = buildArgs with { ProjectName = projectName };
            buildArgs = ExpandBuildArgs(buildArgs, extraProperties, extraItems);

            if (dotnetWasmFromRuntimePack == null)
                dotnetWasmFromRuntimePack = !(buildArgs.AOT || buildArgs.Config == "Release");
            string[] testedCultures = testedCulturesStr.Replace(", ", string.Empty).Split("\"", StringSplitOptions.RemoveEmptyEntries);

            string programText = $@"
                using System;
                using System.Globalization;
                using System.Text;

                try
                {{
                    string[] testedCultures = new string[ {testedCultures.Length} ] {{ {testedCulturesStr} }};
                    foreach (var testedCulture in testedCultures)
                    {{
                        var culture = new CultureInfo(testedCulture, false);
                        Console.WriteLine($""{{culture.NativeName}} - {{culture.DateTimeFormat.FullDateTimePattern}} - {{culture.CompareInfo.LCID}}"");
                    }}
                    string s = new string( new char[] {{'\u0063', '\u0301', '\u0327', '\u00BE'}});
                    string normalized = s.Normalize();
                    Console.WriteLine($""{{normalized.IsNormalized(NormalizationForm.FormC)}}"");

                }}
                catch (CultureNotFoundException e)
                {{
                    Console.WriteLine($""Culture Not Found {{e.Message}}"");
                }}
                return 42;
            "; // missing check: culture.NumberFormat.CurrencySymbol

            BuildProject(buildArgs,
                            id: id,
                            new BuildProjectOptions(
                                InitProject: () => File.WriteAllText(Path.Combine(_projectDir!, "Program.cs"), programText),
                                DotnetWasmFromRuntimePack: dotnetWasmFromRuntimePack,
                                HasInvariantGlobalization: invariantGlobalization != null && invariantGlobalization.Value == true,
                                IcuCultures: declaredIcuCultures));

            string output = RunAndTestWasmApp(buildArgs, expectedExitCode: 42, host: host, id: id);
            if (expectFailure)
            {
                string expectedOutput = "Culture Not Found";
                Assert.Contains(expectedOutput, output);
            }
            else
            {
                for (int i = 0; i < testedCultures.Length; i++)
                {
                    var culture = CultureInfo.GetCultureInfo(testedCultures[i], false);
                    // culture.NativeName is shortened in not-feature shards
                    // e.g. "en (collation=CX)" instead of "English (Sort Order=cx)"
                    // so we cannot get it with {culture.NativeName};
                    var cultureAndCollation = testedCultures[i].Split('_', 2, StringSplitOptions.RemoveEmptyEntries);
                    string expectedOutput = $"{cultureAndCollation[0]} (collation={cultureAndCollation[1]}) - {culture.DateTimeFormat.FullDateTimePattern} - {culture.CompareInfo.LCID}";
                    Assert.Contains(expectedOutput, output);
                }
            }
        }

        public static IEnumerable<object?[]> ICUShardingByFeature_FULL_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    new object?[] { new string[] { "currency" }, "\"de_IT\", \"en_DE\", \"cs_CZ\""},
                    new object?[] { new string[] { "currency" }, "\"hr_HR\", \"es_ES\", \"fr_BE\""}
                    // ToDo: CJK cultures to test when the data contents will start matching
                    )
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        [Theory]
        [MemberData(nameof(ICUShardingByFeature_FULL_Positive), parameters: new object[] { /*aot*/ false, RunHost.All })]
        [MemberData(nameof(ICUShardingByFeature_FULL_Positive), parameters: new object[] { /*aot*/ true, RunHost.All })]
        public void ShardingTestsPositive(BuildArgs buildArgs, string[] declaredIcuFeatures, string testedCultures, RunHost host, string id)
            => TestICUShardingByFeature(buildArgs, declaredIcuFeatures, testedCultures, false, host, id,
                                            extraProperties: "<WasmBuildNative>true</WasmBuildNative>",
                                            dotnetWasmFromRuntimePack: false);

        private void TestICUShardingByFeature(BuildArgs buildArgs,
                             string[] declaredIcuFeatures,
                             string testedCulturesStr,
                             bool? invariantGlobalization,
                             RunHost host,
                             string id,
                             string extraProperties="",
                             bool? dotnetWasmFromRuntimePack=null,
                             bool expectFailure=false)
        {
            if (invariantGlobalization != null)
                extraProperties = $"{extraProperties}<InvariantGlobalization>{invariantGlobalization}</InvariantGlobalization>";

            string extraItems = "";
            foreach (var feature in declaredIcuFeatures)
                extraItems = $"{extraItems}<WasmIcuFeatures Include=\"{feature}\"/>";
            string[] testedCultures = testedCulturesStr.Replace(", ", string.Empty).Split("\"", StringSplitOptions.RemoveEmptyEntries);
            string projectName = $"sharding_{string.Join("-", testedCultures)}";

            buildArgs = buildArgs with { ProjectName = projectName };
            buildArgs = ExpandBuildArgs(buildArgs, extraProperties, extraItems);

            if (dotnetWasmFromRuntimePack == null)
                dotnetWasmFromRuntimePack = !(buildArgs.AOT || buildArgs.Config == "Release");

            string programText = $@"
                using System;
                using System.Globalization;
                using System.Text;

                try
                {{
                    string[] testedCultures = new string[ {testedCultures.Length} ] {{ {testedCulturesStr} }};
                    foreach (var testedCulture in testedCultures)
                    {{
                        var culture = new CultureInfo(testedCulture, false);
                        Console.WriteLine($""{{culture.NativeName}} - {{culture.DateTimeFormat.FullDateTimePattern}} - {{culture.CompareInfo.LCID}}"");
                    }}
                    string s = new string( new char[] {{'\u0063', '\u0301', '\u0327', '\u00BE'}});
                    string normalized = s.Normalize();
                    Console.WriteLine($""{{normalized.IsNormalized(NormalizationForm.FormC)}}"");

                }}
                catch (CultureNotFoundException e)
                {{
                    Console.WriteLine($""Culture Not Found {{e.Message}}"");
                }}
                return 42;
            "; // missing check: culture.NumberFormat.CurrencySymbol

            BuildProject(buildArgs,
                            id: id,
                            new BuildProjectOptions(
                                InitProject: () => File.WriteAllText(Path.Combine(_projectDir!, "Program.cs"), programText),
                                DotnetWasmFromRuntimePack: dotnetWasmFromRuntimePack,
                                HasInvariantGlobalization: invariantGlobalization != null && invariantGlobalization.Value == true,
                                IcuFeatures: declaredIcuFeatures));

            string output = RunAndTestWasmApp(buildArgs, expectedExitCode: 42, host: host, id: id);
            if (expectFailure)
            {
                string expectedOutput = "Culture Not Found";
                Assert.Contains(expectedOutput, output);
            }
            else
            {
                for (int i = 0; i < testedCultures.Length; i++)
                {
                    var culture = CultureInfo.GetCultureInfo(testedCultures[i], false);
                    // culture.NativeName collation has capital letters in WASM, e.g.
                    // English (Sort Order=nz) vs.
                    // English (Sort Order=NZ)
                    int start = culture.NativeName.IndexOf('=');
                    int end = culture.NativeName.IndexOf(')');
                    string nativeNameCapitalized = string.Join("", new string[] {
                        culture.NativeName.Substring(0, start),
                        culture.NativeName.Substring(start, end - start).ToUpper(),
                        culture.NativeName.Substring(end)
                    });
                    string expectedOutput = $"{nativeNameCapitalized} - {culture.DateTimeFormat.FullDateTimePattern} - {culture.CompareInfo.LCID}";
                    Assert.Contains(expectedOutput, output);
                }
            }
        }
    }
}
