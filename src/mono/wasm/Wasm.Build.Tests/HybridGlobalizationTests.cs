// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace Wasm.Build.Tests
{
    public class HybridGlobalizationTests : BuildTestBase
    {
        public HybridGlobalizationTests(ITestOutputHelper output, SharedBuildPerTestClassFixture buildContext)
            : base(output, buildContext)
        {
        }

        public static IEnumerable<object?[]> HybridGlobalizationTestData(bool aot, RunHost host)
            => ConfigWithAOTData(aot)
                .Multiply(
                    new object?[] { GlobalizationMode.Invariant },
                    new object?[] { GlobalizationMode.Hybrid })
                .WithRunHosts(host)
                .UnwrapItemsAsArrays();

        [Theory]
        [MemberData(nameof(HybridGlobalizationTestData), parameters: new object[] { /*aot*/ false, RunHost.All })]
        [MemberData(nameof(HybridGlobalizationTestData), parameters: new object[] { /*aot*/ true, RunHost.All })]
        public void AOT_HybridGlobalization(BuildArgs buildArgs, GlobalizationMode globalizationMode, RunHost host, string id)
            => TestHybridGlobalization(buildArgs, globalizationMode, host, id);

        [Theory]
        [MemberData(nameof(HybridGlobalizationTestData), parameters: new object[] { /*aot*/ false, RunHost.All })]
        public void RelinkingWithoutAOT(BuildArgs buildArgs, GlobalizationMode globalizationMode, RunHost host, string id)
            => TestHybridGlobalization(buildArgs, globalizationMode, host, id,
                                            extraProperties: "<WasmBuildNative>true</WasmBuildNative>",
                                            dotnetWasmFromRuntimePack: false);

        private void TestHybridGlobalization(
            BuildArgs buildArgs,
            GlobalizationMode globalizationMode,
            RunHost host,
            string id,
            string extraProperties="",
            bool? dotnetWasmFromRuntimePack=null)
        {
            string projectName = $"hybrid_{globalizationMode}_{buildArgs.Config}_{buildArgs.AOT}";
            if (dotnetWasmFromRuntimePack == null)
                dotnetWasmFromRuntimePack = !(buildArgs.AOT || buildArgs.Config == "Release");

            extraProperties = $"{extraProperties}<HybridGlobalization>true</HybridGlobalization>";
            // hybrid + invariant should remain invariant
            if (globalizationMode == GlobalizationMode.Invariant)
                extraProperties = $"{extraProperties}<InvariantGlobalization>true</InvariantGlobalization>";

            buildArgs = buildArgs with { ProjectName = projectName };
            buildArgs = ExpandBuildArgs(buildArgs, extraProperties: extraProperties);

            BuildProject(buildArgs,
                            id: id,
                            new BuildProjectOptions(
                                InitProject: () => File.WriteAllText(Path.Combine(_projectDir!, "Program.cs"), s_mainReturns42),
                                DotnetWasmFromRuntimePack: dotnetWasmFromRuntimePack,
                                GlobalizationMode: globalizationMode));

            RunAndTestWasmApp(buildArgs, buildDir: _projectDir, expectedExitCode: 42,
                        test: output => {},
                        host: host, id: id);
        }
    }
}
