// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace Wasm.Build.Tests;

public class ConfigSrcTests : WasmTemplateTestsBase
{
    public ConfigSrcTests(ITestOutputHelper output, SharedBuildPerTestClassFixture buildContext) : base(output, buildContext)
    { }

    // INFO FOR REVIWER:
    // This class can be deleted.
    // It is testing the --config-src argument, which was supposed to be passed to test-main.js
    // but does not make sense in the current form of testing where we are using "dotnet new" templates

    // NOTE: port number determinizes dynamically, so could not generate absolute URI
    [Theory]
    [BuildAndRun()]
    public void ConfigSrcAbsolutePath(string config, bool aot)
    {
        ProjectInfo info = CreateWasmTemplateProject(Template.WasmBrowser, config, aot, "configsrcabsolute");

        UpdateBrowserProgramFile();
        UpdateBrowserMainJs();

        bool IsPublish = false;
        BuildTemplateProject(info,
                        new BuildProjectOptions(
                            config,
                            info.Id,
                            BinFrameworkDir: FindBinFrameworkDir(config, IsPublish),
                            ExpectedFileType: GetExpectedFileType(info, IsPublish),
                            IsPublish: IsPublish
                        ));

        // // await RunBuiltBrowserApp(config, projectFile, extraArgs: "x y z");
        // string frameworkDir = FindBinFrameworkDir(config, forPublish: false);
        // string configSrc = Path.GetFullPath(Path.Combine(frameworkDir, "blazor.boot.json"));

        // // it's trying to run in "AppBundle" directory
        // RunAndTestWasmApp(
        //     buildArgs,
        //     expectedExitCode: 42,
        //     id: id,
        //     frameworkDir: frameworkDir,
        //     extraXHarnessMonoArgs: $"--config-src=\"{configSrc}\"");
    }
}
