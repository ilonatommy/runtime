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

        public static IEnumerable<object?[]> ICUShardingTestData_EFIGS_Positive(bool aot, RunHost host)
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

        public static IEnumerable<object?[]> ICUShardingTestData_CJK_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    new object?[] { new string[] { "zh" }, "\"zh_HK\", \"en_DE\", \"ko_KR\""},
                    new object?[] { new string[] { "en", "ko" }, "\"ja_JP\", \"ko_KR\", \"zh_SG\""},
                    new object?[] { new string[] { "ja" }, "\"en_US\", \"ja_JP\", \"ko_KR\""})
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        public static IEnumerable<object?[]> ICUShardingTestData_no_CJK_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    new object?[] { new string[] { "pl" }, "\"pl_PL\", \"en_MP\", \"da_DK\""},
                    new object?[] { new string[] { "hr", "en" }, "\"hr_BA\", \"mr_IN\", \"fi_FI\""},
                    new object?[] { new string[] { "cs" }, "\"cs_CZ\", \"da_DK\", \"vi_VN\""})
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        public static IEnumerable<object?[]> ICUShardingTestData_EFIG_CJK_Positive(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    new object?[] { new string[] { "fr", "ja" }, "\"fr_CH\", \"ja_JP\", \"zh_HK\""},
                    new object?[] { new string[] { "es", "zh" }, "\"es_419\", \"zh_HK\", \"de_LI\""},
                    new object?[] { new string[] { "en", "ko", "it" }, "\"it_IT\", \"ko_KR\", \"en_TV\""})
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        [Theory]
        [MemberData(nameof(ICUShardingTestData_EFIGS_Positive), parameters: new object[] { /*aot*/ false, RunHost.All })]
        [MemberData(nameof(ICUShardingTestData_EFIGS_Positive), parameters: new object[] { /*aot*/ true, RunHost.All })]
        [MemberData(nameof(ICUShardingTestData_CJK_Positive), parameters: new object[] { /*aot*/ false, RunHost.NodeJS })] // for Chrome fails
        [MemberData(nameof(ICUShardingTestData_CJK_Positive), parameters: new object[] { /*aot*/ true, RunHost.NodeJS })]
        [MemberData(nameof(ICUShardingTestData_no_CJK_Positive), parameters: new object[] { /*aot*/ false, RunHost.All })]
        [MemberData(nameof(ICUShardingTestData_no_CJK_Positive), parameters: new object[] { /*aot*/ true, RunHost.All })]
        [MemberData(nameof(ICUShardingTestData_EFIG_CJK_Positive), parameters: new object[] { /*aot*/ false, RunHost.NodeJS })] // for Chrome fails
        [MemberData(nameof(ICUShardingTestData_EFIG_CJK_Positive), parameters: new object[] { /*aot*/ true, RunHost.NodeJS })]
        public void ShardingTestsPositive(BuildArgs buildArgs, string[] declaredIcuCultures, string testedCultures, RunHost host, string id)
            => TestICUSharding(buildArgs, declaredIcuCultures, testedCultures, true, false, host, id,
                                            extraProperties: "<WasmBuildNative>true</WasmBuildNative>",
                                            dotnetWasmFromRuntimePack: false);

        private void TestICUSharding(BuildArgs buildArgs,
                             string[] declaredIcuCultures,
                             string testedCulturesStr,
                             bool? enableSharding,
                             bool? invariantGlobalization,
                             RunHost host,
                             string id,
                             string extraProperties="",
                             bool? dotnetWasmFromRuntimePack=null)
        {
            string projectName = $"sharding_{string.Join("-", declaredIcuCultures)}";
            if (invariantGlobalization != null)
                extraProperties = $"{extraProperties}<InvariantGlobalization>{invariantGlobalization}</InvariantGlobalization>";
            if (enableSharding != null)
                extraProperties = $"{extraProperties}<EnableSharding>{enableSharding}</EnableSharding>";

            string extraItems = "";
            foreach (var culture in declaredIcuCultures)
                extraItems = $"{extraItems}<WasmIcuCulture Include=\"{culture}\"/>";

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
                                HasIcuSharding: enableSharding != null && enableSharding.Value == true,
                                IcuCulture: declaredIcuCultures));

            string output = RunAndTestWasmApp(buildArgs, expectedExitCode: 42, host: host, id: id);
            for (int i = 0; i < testedCultures.Length; i++)
            {
                var culture = CultureInfo.GetCultureInfo(testedCultures[i], false);
                // culture.NativeName is shortened in wasm app:
                // e.g. "en (collation=CX)" instead of "English (Sort Order=cx)"
                // so we cannot get it with {culture.NativeName};
                // other differences: "en" lacks tt (AM/PM) in the format in some cultures;
                var cultureAndCollation = testedCultures[i].Split('_', 2, StringSplitOptions.RemoveEmptyEntries);
                string expectedOutput = $"{cultureAndCollation[0]} (collation={cultureAndCollation[1]}) - {culture.DateTimeFormat.FullDateTimePattern} - {culture.CompareInfo.LCID}";
                Assert.Contains(expectedOutput, output);
            }
        }
    }
}
