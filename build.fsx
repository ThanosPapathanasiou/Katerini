open System
open System.Diagnostics
open System.IO

let os = Environment.OSVersion.Platform
if os <> PlatformID.Win32NT then
    printfn """
This is not tested in anything other than windows.
If you *really* want to run it then open the script and delete lines 5-12.
Proceed at your own risk.
"""
    exit 1 

// get the first argument passed
let argument    : string = fsi.CommandLineArgs |> Array.tail |> Array.head
let solutionDir : string = __SOURCE_DIRECTORY__

let execute command arguments =
    let startInfo = ProcessStartInfo()
    startInfo.FileName <- command
    startInfo.Arguments <- arguments
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.UseShellExecute <- false
    startInfo.CreateNoWindow <- true

    use proc = new Process()
    proc.StartInfo <- startInfo

    proc.Start() |> ignore
    let output = proc.StandardOutput.ReadToEnd()
    let errors = proc.StandardError.ReadToEnd()
    proc.WaitForExit()

    (output, errors, proc.ExitCode)

let db () =
    printfn "Initializing database..."
    let dbUpProject = Path.Combine(solutionDir, "source", "Katerini.Database", "Katerini.Database.csproj")
    let output, errors, exitCode = execute "dotnet" $"run --project {dbUpProject}"
    // printfn "%s" output
    if not (String.IsNullOrWhiteSpace(errors)) then
        printfn "Errors:\n%s" errors
    if exitCode = 0 then
        printfn "Database upgrade ran successfully."
    else
        printfn "Database upgrade failed to run with exit code: %d" exitCode
    ()

let build () =
    printfn "Running: docker build --file source\Katerini.Website\Dockerfile --tag katerini.website:latest ."
    let dockerfile = Path.Combine(solutionDir, "source", "Katerini.Website", "Dockerfile")
    let imagetag   = "katerini.website:latest"
    let output, errors, exitCode = execute "docker" $"build -q --file {dockerfile} --tag {imagetag} ."
    if not (String.IsNullOrWhiteSpace(errors)) then
        printfn "Error building Docker image: %s" errors
        exit 1
    printfn "Docker image %s built successfully." imagetag

    printfn "Running: docker build --file source\Katerini.Service\Dockerfile --tag katerini.service:latest ."
    let dockerfile = Path.Combine(solutionDir, "source", "Katerini.Service", "Dockerfile")
    let imagetag   = "katerini.service:latest"
    let output, errors, exitCode = execute "docker" $"build -q --file {dockerfile} --tag {imagetag} ."
    if not (String.IsNullOrWhiteSpace(errors)) then
        printfn "Error building Docker image: %s" errors
        exit 1
    printfn "Docker image %s built successfully." imagetag
    ()

let run () =
    printfn "Running: docker-compose up -d"
    let output, errors, exitCode = execute "docker-compose" "up -d"
    if not (String.IsNullOrWhiteSpace(errors)) then
        printfn "Error running docker-compose: %s" errors
        exit 1
    printfn "All done!"
    ()

let stop () =
    printfn "Running: docker-compose down"
    let output, errors, exitCode = execute "docker-compose" "down"
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

match argument.ToLowerInvariant() with
| "db"    -> db ()
| "build" -> build ()
| "run"   -> build () ; run ()
| "stop"  -> stop ()
| "help"  -> help ()
| _ ->
    printfn """Command not recognized."""
    help ()
    exit 1

exit 0