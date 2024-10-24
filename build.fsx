open System
open System.Diagnostics
open System.IO

let choice : string = try fsi.CommandLineArgs |> Array.tail |> Array.head with _ -> "help"

let execute command arguments =
    async {
        use proc = new Process()
        proc.StartInfo <- ProcessStartInfo(
        FileName               = command,
        Arguments              = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError  = true,
        UseShellExecute        = false,
        CreateNoWindow         = true)

        proc.Start() |> ignore
        let! outputTask = proc.StandardOutput.ReadToEndAsync() |> Async.AwaitTask
        let! errorTask  = proc.StandardError.ReadToEndAsync() |> Async.AwaitTask
        proc.WaitForExit()
        return (outputTask, errorTask, proc.ExitCode)
    } |> Async.RunSynchronously

let db () =
    printfn "Initializing database..."
    let dbUpProject = Path.Combine(__SOURCE_DIRECTORY__, "source", "Katerini.Database", "Katerini.Database.csproj")
    let output, errors, exitCode = execute "dotnet" $"run --project {dbUpProject}"
    if not (String.IsNullOrWhiteSpace(errors)) then
        printfn $"Errors:{Environment.NewLine}%s{errors}"
    if exitCode = 0 then
        printfn "Database upgrade ran successfully."
    else
        printfn $"Database upgrade failed to run with exit code: %d{exitCode}"
    ()

let build () =
    printfn "Running: docker build --file source\Katerini.Website\Dockerfile --tag katerini.website:latest ."
    let dockerfile = Path.Combine(__SOURCE_DIRECTORY__, "source", "Katerini.Website", "Dockerfile")
    let imagetag   = "katerini.website:latest"
    let output, errors, exitCode = execute "docker" $"build -q --file {dockerfile} --tag {imagetag} ."
    if exitCode <> 0 then
        printfn $"Error building Docker image: %s{errors}"
        exit 1
    printfn $"Docker image %s{imagetag} built successfully."

    printfn "Running: docker build --file source\Katerini.Service\Dockerfile --tag katerini.service:latest ."
    let dockerfile = Path.Combine(__SOURCE_DIRECTORY__, "source", "Katerini.Service", "Dockerfile")
    let imagetag   = "katerini.service:latest"
    let output, errors, exitCode = execute "docker" $"build -q --file {dockerfile} --tag {imagetag} ."
    if exitCode <> 0 then
        printfn $"Error building Docker image: %s{errors}"
        exit 1
    printfn $"Docker image %s{imagetag} built successfully."
    ()

let run () =
    printfn "Creating a common docker network."
    let output, errors, exitCode = execute "docker" "network create -d bridge shared_network"

    printfn "Running: 'docker-compose up -d' This may take a while as it's downloading docker images."
    let output, errors, exitCode = execute "docker-compose" "-f ./infrastructure/localhost/infra/docker-compose.yml up -d"
    if exitCode <> 0 then
        printfn $"Error running docker-compose: %s{errors}"
        exit 1
        
    let output, errors, exitCode' = execute "docker-compose" "-f ./infrastructure/localhost/katerini/docker-compose.yml up -d"
    if exitCode <> 0 then
        printfn $"Error running docker-compose: %s{errors}"
        exit 1
    printfn "All done!"
    printfn """
website   : http://website.katerini.local
caching   : http://caching.katerini.local   | no authentication needed.
messaging : http://messaging.katerini.local | username / password is guest
logging   : http://logs.katerini.local      | no authentication needed.
"""
    ()

let stop () =
    printfn "Running: docker-compose down"
    let output, errors, exitCode = execute "docker-compose" "-f ./infrastructure/localhost/katerini/docker-compose.yml down"
    let output, errors, exitCode = execute "docker-compose" "-f ./infrastructure/localhost/infra/docker-compose.yml down"
    printfn "All done!"
    ()

let help () =
    printfn """Available options:
db    - uses the Katerini.Database project to initialize the database and update its schema
build - builds all the docker files 
run   - docker-compose up -d
stop  - docker-compose down
help  - displays this helpful message
"""
    ()

match choice.ToLowerInvariant() with
| "db"              -> db ()
| "build"           -> build ()
| "start" | "run"   -> build () ; run ()
| "stop"            -> stop ()
| "help"            -> help ()
| _                 -> printfn """Command not recognized.""" ; help () ; exit 1

exit 0