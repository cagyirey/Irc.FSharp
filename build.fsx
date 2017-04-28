#I @"packages/build/FAKE/tools"
#r @"FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper

open System
open System.IO

type Project = 
    { Name: string
      Summary: string
      Guid: string }

let solutionName = "Irc.FSharp"

let mainProject = 
    { Name = solutionName
      Summary = "An IRC client library for F#."
      Guid = "694ab3b0-8929-4f78-ab72-55f29eb48a36" }

let releaseNotes = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "RELEASE_NOTES.md")

let solutionFile = solutionName + ".sln"
let projects = [ mainProject ]

let buildDir = "./bin"

let isAppveyorBuild = (environVar >> isNull >> not) "APPVEYOR" 
let appveyorBuildVersion = sprintf "%s-a%s" releaseNotes.AssemblyVersion (DateTime.UtcNow.ToString "yyMMddHHmm")

Target "Clean" (fun () ->
    CleanDirs [buildDir]
)

Target "AppveyorBuildVersion" (fun () ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" appveyorBuildVersion) |> ignore
)

Target "AssemblyInfo" (fun () ->
    List.iter(fun project -> 
        let filename = "./src" @@ project.Name @@ "AssemblyInfo.fs"
        CreateFSharpAssemblyInfo filename
            [ Attribute.Title project.Name
              Attribute.Product solutionName
              Attribute.Description project.Summary
              Attribute.Version releaseNotes.AssemblyVersion
              Attribute.FileVersion releaseNotes.AssemblyVersion
              Attribute.Guid project.Guid ]) projects
)

Target "CopyLicense" (fun () ->
    [ "LICENSE.md" ]
    |> CopyTo (buildDir @@ "Release")
)

Target "Build" (fun () ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "All" DoNothing

"Clean"
    =?> ("AppveyorBuildVersion", isAppveyorBuild)
    ==> "AssemblyInfo"
    ==> "CopyLicense"
    ==> "Build"
    ==> "All"

let target = getBuildParamOrDefault "target" "All"

RunTargetOrDefault target