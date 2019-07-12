// -----------------------------------------------------------------------------
// FAKE build script
// -----------------------------------------------------------------------------

#if !FAKE
  #r "Facades/netstandard"
#endif

#r "paket: 
nuget Fake.Core.Target prerelease
nuget Fake.Core.ReleaseNotes
nuget Fake.DotNet.Cli
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.IO.FileSystem  //"

#load "./.fake/build.fsx/intellisense.fsx"

open System
open System.IO

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators

// -----------------------------------------------------------------------------
// Build Properties
// -----------------------------------------------------------------------------

type Project = 
    { Name: string
      Authors: string list
      Summary: string
      Guid: string }

let mainProject = 
    { Name = "Irc.FSharp"
      Summary = "An IRC client library for F#."
      Authors = ["cagyirey"]
      Guid = "694ab3b0-8929-4f78-ab72-55f29eb48a36" }

let configuration = DotNet.BuildConfiguration.Release

// publishable projects - for generated lib info
let projects = [ mainProject ]

let appReferences = !! "*.sln"
let testAssemblies = !! "tests/**/*.*proj"
let nuspecTemplates = !! "src/**/*.nuspec"

let releaseNotes = ReleaseNotes.parse (File.ReadLines "RELEASE_NOTES.md")
let tags = "irc ircv3"

let isAppveyorBuild = Environment.hasEnvironVar "APPVEYOR"
let appveyorBuildVersion = sprintf "%s-a%s" releaseNotes.AssemblyVersion (DateTime.UtcNow.ToString "yyMMddHHmm")

let nugetPublishParams (p: NuGet.NuGetParams) =
    { p with
        Version = releaseNotes.AssemblyVersion
        Tags = tags
        Authors = mainProject.Authors
        Project = mainProject.Name
        Summary = mainProject.Summary
        Description = mainProject.Summary
        WorkingDir = "./"
        OutputPath = "./publish"
        ToolPath = "nuget"
        Files = 
            [ (@"src/Irc.FSharp/bin/Release/**/*.dll", Some "lib", None) ]
        Dependencies = 
            [ "FParsec", NuGet.GetPackageVersion "./packages" "FParsec"] 
    }

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
              AssemblyInfo.Product project.Name
              AssemblyInfo.Description project.Summary
              AssemblyInfo.Version releaseNotes.AssemblyVersion
              AssemblyInfo.FileVersion releaseNotes.AssemblyVersion
              AssemblyInfo.Guid project.Guid ]) projects
)

Target.create "RunTests" (fun _ -> 
    testAssemblies
    |> Seq.iter (DotNet.test (fun cfg ->
        { cfg with
            NoBuild = true
            Configuration = configuration
        })
    )
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
    |> Seq.iter (DotNet.build (fun cfg ->
        { cfg with
            Configuration = configuration
        })
    )
)

Target.create "NugetPublish" (fun _ ->
    nuspecTemplates
    |> Seq.iter (NuGet.NuGet nugetPublishParams)
)

Target.create "All" ignore

// -----------------------------------------------------------------------------
// Build order
// -----------------------------------------------------------------------------

"Clean"
    =?> ("AppveyorBuildVersion", isAppveyorBuild)
    ==> "AssemblyInfo"
    // paket restore is run by the build script and possibly netcore as well
    // preferable to do it as a build task if other triggers can be disabled
    // ==> "Restore" 
    ==> "Build"
    ==> "RunTests"
    ==> "All"

"All"
    ==> "NuGetPublish"

Target.runOrDefaultWithArguments "All"