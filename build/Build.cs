using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotCover;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[UnsetVisualStudioEnvironmentVariables]
[DotNetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
[TeamCitySetDotCoverHomePath]
[GitHubActions(
    "deployment",
    GitHubActionsImage.MacOsLatest,
    OnPushBranches = new[] { TrunkBranch, ReleaseBranchPrefix + "/*" },
    InvokedTargets = new[] { nameof(Publish) }
        //ImportGitHubTokenAs = nameof(GitHubToken),
        //ImportSecrets =
        //    new[]
        //    {
        //nameof(NuGetApiKey),
        //nameof(SlackWebhook),
        //nameof(GitterAuthToken),
        //nameof(TwitterConsumerKey),
        //nameof(TwitterConsumerSecret),
        //nameof(TwitterAccessToken),
        //nameof(TwitterAccessTokenSecret)
        //    }
        )]
[TeamCity(
    Version = "2020.1",
    VcsTriggeredTargets = new[] { nameof(Pack), nameof(Test) },
    NightlyTriggeredTargets = new[] { nameof(ExecuteCLI) },
    ManuallyTriggeredTargets = new[] { nameof(Publish) },
    NonEntryTargets = new[] { nameof(Restore), nameof(DownloadFonts), nameof(InstallFonts), nameof(ReleaseImage) },
    ExcludedTargets = new[] { nameof(Clean), nameof(SignPackages) })]
[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.MacOsLatest,
    OnPushBranchesIgnore = new[] { TrunkBranch, ReleaseBranchPrefix + "/*" },
    OnPullRequestBranches = new[] { DevelopBranch },
    PublishArtifacts = false,
    InvokedTargets = new[] { nameof(Test), nameof(Pack) })]
[AppVeyor(
    AppVeyorImage.VisualStudio2019,
    AppVeyorImage.Ubuntu1804,
    AutoGenerate = false,
    SkipTags = true,
    InvokedTargets = new[] { nameof(Test), nameof(Pack) })]
[AzurePipelines(
    suffix: null,
    AzurePipelinesImage.UbuntuLatest,
    AzurePipelinesImage.WindowsLatest,
    AzurePipelinesImage.MacOsLatest,
    InvokedTargets = new[] { nameof(Test), nameof(Pack) },
    NonEntryTargets = new[] { nameof(Restore), nameof(DownloadFonts), nameof(InstallFonts), nameof(ReleaseImage) },
    ExcludedTargets = new[] { nameof(Clean), nameof(Coverage), nameof(SignPackages) })]
partial class Build : NukeBuild
{
    /// <summary>
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    const string TrunkBranch = "trunk";
    const string DevelopBranch = "develop";
    const string ReleaseBranchPrefix = "release";
    const string HotfixBranchPrefix = "hotfix";

    [LocalExecutable("output/SampleConsole/SampleConsole.exe")]
    readonly Tool SampleConsole;
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Test");
        });

    string ChangelogFile => RootDirectory / "CHANGELOG.md";
    AbsolutePath PackageDirectory => OutputDirectory / "packages";
    IReadOnlyCollection<AbsolutePath> PackageFiles => PackageDirectory.GlobFiles("*.nupkg");
    Target Pack => _ => _
    .DependsOn(Compile)
        .Produces(PackageDirectory / "*.nupkg")
        .Executes(() =>
        {
            Serilog.Log.Information("Pack");
            DotNetPack(_ => _
                .SetProject(Solution)
                .SetNoBuild(InvokedTargets.Contains(Compile))
                .SetConfiguration(Configuration)
                .SetOutputDirectory(PackageDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetPackageReleaseNotes(GetNuGetReleaseNotes(ChangelogFile, GitRepository)));
        });

    Target Coverage => _ => _
    .Executes(() =>
    {
        Serilog.Log.Information("Coverage");
    });

    Target SignPackages => _ => _
    .Executes(() =>
    {
        Serilog.Log.Information("SignPackages");
    });

    Target Publish => _ => _
    .Executes(() =>
    {
        Serilog.Log.Information("Publish");
    });
    Target PublishCLI => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetPublish(d => d
                .SetProject(SourceDirectory / "TranslationConsole")
                .SetOutput(OutputDirectory / "TranslationConsole")
                );
        });

    Target ExecuteCLI => _ => _
     .DependsOn(PublishCLI)
     .Executes(() =>
     {
         var someDir = RootDirectory / "SomeStuff";
         SampleConsole(@"--someDir '{someDir}'");
         Serilog.Log.Information("The stuff just happened?");
     });
}
