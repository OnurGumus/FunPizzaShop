﻿open Fake.Core
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Client"
let clientDeployPath = Path.combine clientPath "deploy"
let deployDir = Path.getFullName "./deploy"
let clientDeployReleasePath = "clientFiles" |> Path.combine deployDir
let automationPath = Path.getFullName "./test/Automation"

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool

    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool
            + " was not found in path. "
            + "Please install it and make sure it's available from your path. "
            + "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"

        failwith errorMsg

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"
let npmTool = platformTool "npm" "npm.cmd"

let runTool procStart cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    RawCommand(cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> procStart

let runDotNet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""

    if result.ExitCode <> 0 then
        failwithf "'dotnet %s' failed in %s" cmd workingDir

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

let initTargets () =
    Target.create "Clean" (fun _ -> [ deployDir; clientDeployPath ] |> Shell.cleanDirs)
    "Clean" |> ignore

    Target.create "InstallClient" (fun _ ->
        let runTool = runTool Proc.run
        printfn "Node version:"
        runTool npmTool "ci" "."  |> ignore)
        // printfn "Yarn version:"
        // runTool yarnTool "--version" __SOURCE_DIRECTORY__ |>ignore
        // runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__ |> ignore)

    Target.create "BuildClient" (fun _ ->
        let runTool = runTool Proc.run
        runTool npmTool "run postcss" "." |> ignore
        runTool npmTool "run build" clientPath |> ignore
    )

    Target.create "Build" (fun _ ->
        runDotNet "build --configuration release " serverPath
        runDotNet "fable watch ./src/Client  --run yarn prod" __SOURCE_DIRECTORY__)

    Target.create "PublishServer" (fun _ ->
        Shell.cleanDir deployDir
        runDotNet ("publish --configuration Release --output " + deployDir) serverPath)


    Target.create "RunServer" (fun _ ->
        let runTool = runTool Proc.run
        runDotNet "watch run" serverPath 
     )

    Target.create "RunAutomation" (fun _ ->
        Process.setKillCreatedProcesses true
        async {
                let uiTask = (runTool Proc.startRaw) "dotnet" "run" automationPath
                let ExitCode = uiTask.Result.Raw.Result.RawExitCode 
                Process.killAllCreatedProcesses()
                if ExitCode <> 0 then
                    failwith "UITest failed"
        }
        |> Async.RunSynchronously
        |> ignore)

    Target.create "PostCSS" (fun _ ->
        let runTool = runTool Proc.startRaw
        runTool "npm" "run postcss-watch" "." |> ignore)

   
    Target.create "Run" (fun _ ->
        let runTool = runTool Proc.run
        let server = async { runDotNet "watch run" serverPath }

        let postcss = async {
             (runTool)  "npm run postcss-watch" "." |> ignore
        }
        
        let browser = async {
            do! Async.Sleep 5000
            openBrowser "http://localhost:8080"
        }

        let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
        let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

        let tasks = [postcss; server; browser ]

        tasks |> Async.Parallel |> Async.RunSynchronously |> ignore)


    Target.create "RunClient" (fun _ ->
        let runTool = runTool Proc.run
        let server = async { return () }

       
        let browser = async {
            do! Async.Sleep 5000
            openBrowser "http://localhost:8080"
        }

        let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
        let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

        let tasks = [ server; browser ]

        tasks |> Async.Parallel |> Async.RunSynchronously |> ignore)

    "Clean" ==> "InstallClient" ==> "Build" |> ignore
    "Clean" ==> "InstallClient" ==> "Run" |> ignore
    "Clean" ==> "InstallClient" ==> "BuildClient" ==> "PublishServer"|> ignore
    "RunServer"

[<EntryPoint>]
let main argv =
    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets () |> ignore
    Target.runOrDefaultWithArguments "RunServer"

    0 // return an integer exit cod
