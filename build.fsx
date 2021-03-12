#r "paket:
nuget BlackFox.Fake.BuildTask
nuget Fake.Core.Target
nuget Fake.Core.Process
nuget Fake.Core.ReleaseNotes
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Paket
nuget Fake.DotNet.FSFormatting
nuget Fake.DotNet.Fsi
nuget Fake.DotNet.NuGet
nuget Fake.Api.Github
nuget Fake.DotNet.Testing.Expecto 
nuget Fake.Extensions.Release
nuget Fake.Tools.Git //
"

#if !FAKE
#load "./.fake/build.fsx/intellisense.fsx"
#r "netstandard" // Temp fix for https://github.com/dotnet/fsharp/issues/5216
#endif


open System
open System.Text
open Fake
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.Api
open Fake.Tools.Git

Target.initEnvironment ()

let sharedPath = Path.getFullName "./src/Shared"
let serverPath = Path.getFullName "./src/Server"
let deployDir = Path.getFullName "./deploy"
let sharedTestsPath = Path.getFullName "./tests/Shared"
let serverTestsPath = Path.getFullName "./tests/Server"


let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool + " was not found in path. " +
            "Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
        failwith errorMsg

let nodeTool = platformTool "node" "node.exe"
let npmTool = platformTool "npm" "npm.cmd"
let npxTool = platformTool "npx" "npx.cmd"

let currentDateString =
    let n = System.DateTime.Now
    sprintf "%i-%i-%i" n.Year n.Month n.Day

let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let npm args workingDir =
    let npmPath =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            |> failwith

    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    Command.RawCommand (npmPath, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let dotnet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

Target.create "InstallWordAddinTooling" (fun _ ->

    printfn "Installing office addin tooling"

    runTool npmTool "install -g office-addin-dev-certs" __SOURCE_DIRECTORY__
    runTool npmTool "install -g office-addin-debugging" __SOURCE_DIRECTORY__
    runTool npmTool "install -g office-addin-manifest" __SOURCE_DIRECTORY__
)

Target.create "WebpackConfigSetup" (fun _ ->
    let userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)

    Shell.replaceInFiles
        [
            "{USERFOLDER}",userPath.Replace("\\","/")
        ]
        [
            (Path.combine __SOURCE_DIRECTORY__ "webpack.config.js")
        ]
)

Target.create "SetLoopbackExempt" (fun _ ->
    Command.RawCommand("CheckNetIsolation.exe",Arguments.ofList [
        "LoopbackExempt"
        "-a"
        "-n=\"microsoft.win32webviewhost_cw5n1h2txyewy\""
    ])
    |> CreateProcess.fromCommand
    |> Proc.run
    |> ignore
)

Target.create "CreateDevCerts" (fun _ ->
    runTool npxTool "office-addin-dev-certs install --days 365" __SOURCE_DIRECTORY__

    let certPath =
        Path.combine
            (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            ".office-addin-dev-certs/ca.crt"
        

    let psi = new System.Diagnostics.ProcessStartInfo(FileName = certPath, UseShellExecute = true)
    System.Diagnostics.Process.Start(psi) |> ignore
)

Target.create "OfficeDebug" (fun _ ->
    let server = async {
        dotnet "watch run" serverPath
    }
    let officeDebug = async {
         runTool npxTool "office-addin-debugging start manifest.xml desktop --debug-method web" __SOURCE_DIRECTORY__
    }
    let client = async {
        runTool npxTool "webpack-dev-server" __SOURCE_DIRECTORY__
    }

    //let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
    let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

    let tasks =
        [
          yield officeDebug 
          yield client
          if not safeClientOnly then yield server
          ]
    tasks
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

open Fake.Extensions.Release

module ProjectInfo =
    
    let gitOwner = "nfdi4plants"
    let gitName = "Spawn"

module ReleaseNoteTasks =

    open Fake.Extensions.Release

    let createAssemblyVersion = Target.create "createvfs" (fun e ->
        AssemblyVersion.create ProjectInfo.gitName
    )

    let updateReleaseNotes = Target.create "Release" (fun config ->
        Release.exists()

        Release.update(ProjectInfo.gitOwner, ProjectInfo.gitName, config)

        Trace.trace "Update Version.fs"

        let newRelease = ReleaseNotes.load "RELEASE_NOTES.md"

        let releaseDate =
            if newRelease.Date.IsSome then newRelease.Date.Value.ToShortDateString() else "WIP"

        Fake.DotNet.AssemblyInfoFile.createFSharp  "src/Server/Version.fs"
            [   Fake.DotNet.AssemblyInfo.Title "SPAWN"
                Fake.DotNet.AssemblyInfo.Version newRelease.AssemblyVersion
                Fake.DotNet.AssemblyInfo.Metadata ("ReleaseDate",releaseDate)
            ]

        Trace.trace "Update Version.fs done!"

        Trace.trace "Update manifest.xml"

        let _ =
            let newVer = sprintf "<Version>%i.%i.%i</Version>" newRelease.SemVer.Major newRelease.SemVer.Minor newRelease.SemVer.Patch
            Shell.regexReplaceInFilesWithEncoding
                "<Version>[0-9]+.[0-9]+.[0-9]+</Version>"
                newVer
                Encoding.UTF8
                [
                    (Path.combine __SOURCE_DIRECTORY__ @".assets\assets\manifest.xml")
                    (Path.combine __SOURCE_DIRECTORY__ "manifest.xml")
                ]
        Trace.trace "Update manifest.xml done!"
    )

    let githubDraft = Target.create "GithubDraft" (fun config ->

        let bodyText =
            [
                ""
                ""
                "You can check our [release notes](https://github.com/nfdi4plants/Spawn/blob/master/RELEASE_NOTES.md) to see a list of all new features."
                "If you decide to test Spawn in the current state, please take the time to set up a Github account to report your issues and suggestions here."
                ""
                "You can also search existing issues for solutions for your questions and/or discussions about your suggestions."
                ""
                "Here are the necessary steps to use Spawn:"
                ""
                "#### If you use the excel desktop application locally:"
                "    - Install node.js LTS (needed for office add-in related tooling)"
                "    - Download the release archive (.zip file) below and extract it"
                "    - Execute the swate.cmd (windows) or swate.sh (macOS, you will need to make it executable via chmod a+x) script."
                ""
                "#### If you use Excel in the browser:"
                "    - Download the release archive (.zip file) below and extract it"
                "    - Launch Excel online, open a (blank) workbook"
                "    - Under the Insert tab, select Add-Ins"
                "    - Go to Manage my Add-Ins and select Upload my Add-In"
                ""
                "The latest release features:"
                ""
            ] |> String.concat System.Environment.NewLine

        let assetBasePath = System.IO.Path.Combine(__SOURCE_DIRECTORY__,@".assets")
        let assetPath = System.IO.Path.Combine(assetBasePath,@"assets")
        Github.zipAssets(assetPath,assetBasePath,"spawn.zip")

        let draft(owner:string, repoName:string, releaseBody:string option, assetPath: string option, config:TargetParameter) =

            let prevReleaseNotes = Fake.IO.File.read "RELEASE_NOTES.md"
            let takeLastOfReleaseNotes =
                let findInd =
                    prevReleaseNotes
                    |> Seq.indexed
                    |> Seq.choose (fun (i,x) -> if x.StartsWith "###" then Some i else None)
                match Seq.length findInd with
                | 1 ->
                    prevReleaseNotes
                | x when x >= 2 ->
                    let indOfSecondLastRN = findInd|> Seq.item 1
                    Seq.take (indOfSecondLastRN - 1) prevReleaseNotes
                | _ ->
                    failwith "Previous RELEASE_NOTES.md not found or in wrong formatting"

            let bodyText =
                [
                    if releaseBody.IsSome then releaseBody.Value
                    yield! takeLastOfReleaseNotes
                ]

            let tokenOpt =
                config.Context.Arguments
                |> List.tryFind (fun x -> x.StartsWith "token:")

            let release = ReleaseNotes.load "RELEASE_NOTES.md"
            let semVer = (sprintf "v%i.%i.%i" release.SemVer.Major release.SemVer.Minor release.SemVer.Patch)

            let token =
                match Environment.environVarOrDefault "github_token" "", tokenOpt with
                | s, None when System.String.IsNullOrWhiteSpace s |> not -> s
                | s, Some token when System.String.IsNullOrWhiteSpace s |> not ->
                    Trace.traceImportant "Environment variable for token and token argument found. Will proceed with token passed as argument 'token:my-github-token'"
                    token.Replace("token:","")
                | _, Some token ->
                    token.Replace("token:","")
                | _, None ->
                    failwith "please set the github_token environment variable to a github personal access token with repro access or pass the github personal access token as argument as in 'token:my-github-token'."

            let files (assetPath:string) =
                let assetDir = Fake.IO.DirectoryInfo.ofPath assetPath
                /// This only accesses files and not folders. So in this case it will only access the .zip file created by "ZipAssets"
                let assetsPaths = Fake.IO.DirectoryInfo.getFiles assetDir
                assetsPaths |> Array.map (fun x -> x.FullName)

            let draft = 
                if assetPath.IsSome then
                    let assetPaths = (files assetPath.Value)
                    GitHub.createClientWithToken token
                    |> GitHub.draftNewRelease owner repoName semVer (release.SemVer.PreRelease <> None) bodyText
                    |> GitHub.uploadFiles assetPaths
                    |> Async.RunSynchronously

                else 
                
                    GitHub.createClientWithToken token
                    |> GitHub.draftNewRelease owner repoName semVer (release.SemVer.PreRelease <> None) bodyText
                    |> Async.RunSynchronously

            Trace.tracef "Draft %A successfully created!" draft.Release.Name

        draft(
            ProjectInfo.gitOwner,
            ProjectInfo.gitName,
            (Some bodyText),
            None,
            config
        )
    )

Target.create "Clean" (fun _ -> Shell.cleanDir deployDir)

Target.create "InstallClient" (fun _ -> npm "install" ".")

Target.create "Bundle" (fun _ ->
    dotnet (sprintf "publish -c Release -o \"%s\"" deployDir) serverPath
    npm "run build" "."
)

Target.create "Setup" ignore 

Target.create "Run" (fun _ ->
    dotnet "build" sharedPath
    [ async { dotnet "watch run" serverPath }
      async { npm "run start" "." } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunTests" (fun _ ->
    dotnet "build" sharedTestsPath
    [ async { dotnet "watch run" serverTestsPath }
      async { npm "run test:live" "." } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

open Fake.Core.TargetOperators

"createvfs"
    ==> "Release"

"InstallWordAddinTooling"
    ==> "WebpackConfigSetup"
    ==> "CreateDevCerts"
    ==> "SetLoopbackExempt"
    ==> "Setup"

"Clean"
    ==> "InstallClient"
    ==> "Run"

"Clean"
    ==> "InstallClient"
    ==> "OfficeDebug"

"Clean"
    ==> "InstallClient"
    ==> "RunTests"

Target.runOrDefaultWithArguments "Bundle"
