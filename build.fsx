// -----------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------
#r "paket: 
nuget Fake.Core.Target prerelease
nuget Fake.Core.ReleaseNotes
nuget Fake.DotNet.Cli
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.IO.FileSystem  //"

//#load "./.fake/build_new.fsx/intellisense.fsx"

open System
open System.IO

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators

// -----------------------------------------------------------------------------
// Build Properties
// --------------------------------------------------------------------------------

type Project = 
    { Name: string
      Summary: string
      Guid: string }

let solutionName = "Irc.FSharp"

let configuration = "Release"

let tags = "irc, ircv3"

let mainProject = 
    { Name = solutionName
      Summary = "An IRC client library for F#."
      Guid = "694ab3b0-8929-4f78-ab72-55f29eb48a36" }

let releaseNotes = ReleaseNotes.parse (File.ReadLines "RELEASE_NOTES.md")

// publishable projects - for generated lib info
let projects = [ mainProject ]

let testAssemblies = "tests/**/*.*proj"

let outputPath = "./bin"

let isAppveyorBuild = Environment.hasEnvironVar "APPVEYOR" 
let appveyorBuildVersion = sprintf "%s-a%s" releaseNotes.AssemblyVersion (DateTime.UtcNow.ToString "yyMMddHHmm")

let appReferences = !! "*.sln"

// -----------------------------------------------------------------------------
// Custom Targets
// -----------------------------------------------------------------------------

Target.create "AppveyorBuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" appveyorBuildVersion) |> ignore
)

Target.create "AssemblyInfo" (fun _ ->
    List.iter(fun project -> 
        let filename = "./src" @@ project.Name @@ "AssemblyInfo.fs"
        AssemblyInfoFile.createFSharp filename
            [ AssemblyInfo.Title project.Name
              AssemblyInfo.Product solutionName
              AssemblyInfo.Description project.Summary
              AssemblyInfo.Version releaseNotes.AssemblyVersion
              AssemblyInfo.FileVersion releaseNotes.AssemblyVersion
              AssemblyInfo.Guid project.Guid ]) projects
)

Target.create "RunTests" (fun _ -> 
    !! testAssemblies
    |> Seq.iter (DotNet.test id)
)

// -----------------------------------------------------------------------------
// Targets
// -----------------------------------------------------------------------------

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "test/**/bin"
    ++ "test/**/obj"
    |> Shell.cleanDirs 
)

Target.create "Restore" (fun _ ->
    appReferences
    |> Seq.iter (fun p -> DotNet.restore id p)
)

Target.create "CopyLicense" (fun _ -> ()
    // todo: implement this
)

Target.create "Build" (fun _ ->
    appReferences
    |> Seq.iter (DotNet.build (fun cfg -> { cfg with 
        OutputPath = Some outputPath}))
)

Target.create "All" ignore

// -----------------------------------------------------------------------------
// Build order
// -------------------------------------------------------------------------------=

"Clean"
    =?> ("AppveyorBuildVersion", isAppveyorBuild)
    ==> "AssemblyInfo"
    // paket restore is run by the build script and possibly netcore as well
    // preferable to do it as a build task if other triggers can be disabled
    // ==> "Restore" 
    ==> "Build"
    ==> "RunTests"
    ==> "All"

Target.runOrDefault "All"