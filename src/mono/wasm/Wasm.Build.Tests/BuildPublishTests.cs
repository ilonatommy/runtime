// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using Wasm.Build.NativeRebuild.Tests;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using System.Collections.Generic;

#nullable enable

namespace Wasm.Build.Tests
{
    public class BuildPublishTests : WasmTemplateTestsBase
    {
        public BuildPublishTests(ITestOutputHelper output, SharedBuildPerTestClassFixture buildContext)
            : base(output, buildContext)
        {
        }

        [Theory]
        [BuildAndRun(config: "Debug", aot: true)]
        public void Wasm_CannotAOT_InDebug(string config, bool aot)
        {
            ProjectInfo info = CreateWasmTemplateProject(Template.WasmBrowser, config, aot, "no_aot_in_debug");

            bool IsPublish = true;
            (string _, string buildOutput) = BuildTemplateProject(info,
                        new BuildProjectOptions(
                            config,
                            info.Id,
                            BinFrameworkDir: FindBinFrameworkDir(config, IsPublish),
                            ExpectedFileType: GetExpectedFileType(info, IsPublish),
                            IsPublish: IsPublish,
                            ExpectSuccess: false
                        ));
            Console.WriteLine($"buildOutput={buildOutput}");
            Assert.Contains("AOT is not supported in debug configuration", buildOutput);

            // string projectName = GetTestProjectPath(prefix: "no_aot_in_debug", config: buildArgs.Configuration);
            // buildArgs = buildArgs with { ProjectName = projectName };
            // buildArgs = ExpandBuildArgs(buildArgs);
            // (string projectDir, string buildOutput) = BuildProject(buildArgs,
            //             id: id,
            //             new BuildProjectOptions(
            //             InitProject: () => File.WriteAllText(Path.Combine(_projectDir!, "Program.cs"), s_mainReturns42),
            //             DotnetWasmFromRuntimePack: true,
            //             CreateProject: true,
            //             Publish: true,
            //             ExpectSuccess: false
            //             ));

            // Console.WriteLine($"buildOutput={buildOutput}");

            // Assert.Contains("AOT is not supported in debug configuration", buildOutput);
        }

        // [Theory]
        // [BuildAndRun(config: "Release")]
        // [BuildAndRun(config: "Debug")]
        // public void BuildThenPublishNoAOT(string config, bool aot)
        // {
        //     ProjectInfo info = CreateWasmTemplateProject(Template.WasmBrowser, config, aot, "build_publish");
        
        //     UpdateBrowserProgramFile();
        //     UpdateBrowserMainJs();

        //     bool isPublish = false;
        //     (string _, string buildOutput) = BuildTemplateProject(info,
        //                 new BuildProjectOptions(
        //                     config,
        //                     info.Id,
        //                     BinFrameworkDir: FindBinFrameworkDir(config, IsPublish),
        //                     ExpectedFileType: GetExpectedFileType(info, IsPublish),
        //                     IsPublish: IsPublish
        //                 ));

        //     if (!_buildContext.TryGetBuildFor(info, out BuildProduct? product))
        //         throw new XunitException($"Test bug: could not get the build product in the cache");

        //     // how to run it in a new way?
        //     await RunBuiltBrowserApp(info.Configuration, info.ProjectFilePath);

        //     // string projectName = GetTestProjectPath(prefix: "build_publish", config: buildArgs.Configuration);

        //     // buildArgs = buildArgs with { ProjectName = projectName };
        //     // buildArgs = ExpandBuildArgs(buildArgs);

        //     // // no relinking for build
        //     // bool relinked = false;
        //     // BuildProject(buildArgs,
        //     //             id: id,
        //     //             new BuildProjectOptions(
        //     //             InitProject: () => File.WriteAllText(Path.Combine(_projectDir!, "Program.cs"), s_mainReturns42),
        //     //             DotnetWasmFromRuntimePack: !relinked,
        //     //             CreateProject: true,
        //     //             Publish: false
        //     //             ));

        //     Run();

        //     if (!_buildContext.TryGetBuildFor(buildArgs, out BuildProduct? product))
        //         throw new XunitException($"Test bug: could not get the build product in the cache");

        //     File.Move(product!.LogFile, Path.ChangeExtension(product.LogFile!, ".first.binlog"));

        //     _testOutput.WriteLine($"{Environment.NewLine}Publishing with no changes ..{Environment.NewLine}");

        //     // relink by default for Release+publish
        //     relinked = buildArgs.Configuration == "Release";
        //     BuildProject(buildArgs,
        //                 id: id,
        //                 new BuildProjectOptions(
        //                     DotnetWasmFromRuntimePack: !relinked,
        //                     CreateProject: false,
        //                     Publish: true,
        //                     UseCache: false));

        //     Run();

        //     void Run() => RunAndTestWasmApp(
        //                         buildArgs, buildDir: _projectDir, expectedExitCode: 42,
        //                         test: output => {},
        //                         host: host, id: id);
        // }

        // [Theory]
        // [BuildAndRun(aot: true, config: "Release")]
        // public void BuildThenPublishWithAOT(ProjectInfo buildArgs, RunHost host, string id)
        // {
        //     bool testUnicode = true;
        //     string projectName = GetTestProjectPath(
        //         prefix: "build_publish", config: buildArgs.Configuration, appendUnicode: testUnicode);

        //     buildArgs = buildArgs with { ProjectName = projectName };
        //     buildArgs = ExpandBuildArgs(buildArgs);

        //     // no relinking for build
        //     bool relinked = false;
        //     (_, string output) = BuildProject(buildArgs,
        //                             id,
        //                             new BuildProjectOptions(
        //                                 InitProject: () => File.WriteAllText(Path.Combine(_projectDir!, "Program.cs"), s_mainReturns42),
        //                                 DotnetWasmFromRuntimePack: !relinked,
        //                                 CreateProject: true,
        //                                 Publish: false,
        //                                 Label: "first_build"));

        //     BuildPaths paths = GetBuildPaths(buildArgs);
        //     var pathsDict = _provider.GetFilesTable(buildArgs, paths, unchanged: false);

        //     string mainDll = $"{buildArgs.ProjectName}.dll";
        //     var firstBuildStat = _provider.StatFiles(pathsDict.Select(kvp => kvp.Value.fullPath));
        //     Assert.False(firstBuildStat["pinvoke.o"].Exists);
        //     Assert.False(firstBuildStat[$"{mainDll}.bc"].Exists);

        //     CheckOutputForNativeBuild(expectAOT: false, expectRelinking: relinked, buildArgs, output, testUnicode);

        //     Run(expectAOT: false);

        //     if (!_buildContext.TryGetBuildFor(buildArgs, out BuildProduct? product))
        //         throw new XunitException($"Test bug: could not get the build product in the cache");

        //     File.Move(product!.LogFile, Path.ChangeExtension(product.LogFile!, ".first.binlog"));

        //     _testOutput.WriteLine($"{Environment.NewLine}Publishing with no changes ..{Environment.NewLine}");

        //     Dictionary<string, FileStat> publishStat = new();
        //     // relink by default for Release+publish
        //     (_, output) = BuildProject(buildArgs,
        //                         id: id,
        //                         new BuildProjectOptions(
        //                             DotnetWasmFromRuntimePack: false,
        //                             CreateProject: false,
        //                             Publish: true,
        //                             UseCache: false,
        //                             Label: "first_publish"));

        //     publishStat = (Dictionary<string, FileStat>)_provider.StatFiles(pathsDict.Select(kvp => kvp.Value.fullPath));
        //     Assert.True(publishStat["pinvoke.o"].Exists);
        //     Assert.True(publishStat[$"{mainDll}.bc"].Exists);
        //     CheckOutputForNativeBuild(expectAOT: true, expectRelinking: false, buildArgs, output, testUnicode);
        //     _provider.CompareStat(firstBuildStat, publishStat, pathsDict.Values);

        //     Run(expectAOT: true);

        //     // second build
        //     (_, output) = BuildProject(buildArgs,
        //                                 id: id,
        //                                 new BuildProjectOptions(
        //                                     InitProject: () => File.WriteAllText(Path.Combine(_projectDir!, "Program.cs"), s_mainReturns42),
        //                                     DotnetWasmFromRuntimePack: !relinked,
        //                                     CreateProject: true,
        //                                     Publish: false,
        //                                     Label: "second_build"));
        //     var secondBuildStat = _provider.StatFiles(pathsDict.Select(kvp => kvp.Value.fullPath));

        //     // no relinking, or AOT
        //     CheckOutputForNativeBuild(expectAOT: false, expectRelinking: false, buildArgs, output, testUnicode);

        //     // no native files changed
        //     pathsDict.UpdateTo(unchanged: true);
        //     _provider.CompareStat(publishStat, secondBuildStat, pathsDict.Values);

        //     void Run(bool expectAOT) => RunAndTestWasmApp(
        //                         buildArgs with { AOT = expectAOT },
        //                         buildDir: _projectDir, expectedExitCode: 42,
        //                         host: host, id: id);
        // }

        // void CheckOutputForNativeBuild(bool expectAOT, bool expectRelinking, ProjectInfo buildArgs, string buildOutput, bool testUnicode)
        // {
        //     if (testUnicode)
        //     {
        //         string projectNameCore = buildArgs.ProjectName.Replace(s_unicodeChars, "");
        //         TestUtils.AssertMatches(@$"{projectNameCore}\S+.dll -> {projectNameCore}\S+.dll.bc", buildOutput, contains: expectAOT);
        //         TestUtils.AssertMatches(@$"{projectNameCore}\S+.dll.bc -> {projectNameCore}\S+.dll.o", buildOutput, contains: expectAOT);
        //     }
        //     else
        //     {
        //         TestUtils.AssertSubstring($"{buildArgs.ProjectName}.dll -> {buildArgs.ProjectName}.dll.bc", buildOutput, contains: expectAOT);
        //         TestUtils.AssertSubstring($"{buildArgs.ProjectName}.dll.bc -> {buildArgs.ProjectName}.dll.o", buildOutput, contains: expectAOT);
        //     }
        //     TestUtils.AssertMatches("pinvoke.c -> pinvoke.o", buildOutput, contains: expectRelinking || expectAOT);
        // }
    }
}
