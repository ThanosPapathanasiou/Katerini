open System
open System.Data
open System.IO
open Katerini.Core.Outbox
open Microsoft.AspNetCore.Builder
open Microsoft.Data.SqlClient
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Giraffe
open Serilog

let webApp =
    choose [
        GET  >=> route  "/"                        >=> Katerini.Website.Pages.Index.``GET /``

        GET  >=> route  "/contact-form"            >=> Katerini.Website.Pages.ContactForm.``GET  /contact-form``
        POST >=> route  "/contact-form"            >=> Katerini.Website.Pages.ContactForm.``POST /contact-form``
        POST >=> route  "/contact-form/email"      >=> Katerini.Website.Pages.ContactForm.``POST /contact-form/email``
        POST >=> route  "/contact-form/firstname"  >=> Katerini.Website.Pages.ContactForm.``POST /contact-form/firstname``
        POST >=> route  "/contact-form/lastname"   >=> Katerini.Website.Pages.ContactForm.``POST /contact-form/lastname``

        GET  >=> route  "/health/live"             >=> setStatusCode 200 >=> text "OK"
        setStatusCode 404 >=> text "Not Found"
    ]

let errorHandler (ex : Exception) (logger : Microsoft.Extensions.Logging.ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    // setup Environment variables
    builder.Environment.ApplicationName <- "Katerini.Website"
    builder.Environment.ContentRootPath <- Directory.GetCurrentDirectory()
    builder.Environment.WebRootPath     <- Path.Combine(builder.Environment.ContentRootPath, "WebRoot")
    
    // setup logging
    let loggerConfig = LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.Seq(serverUrl=builder.Configuration["SeqLogging.Configuration:ServerUrl"])
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                        |> fun lc -> lc.CreateLogger()
    Log.Logger <- loggerConfig
    builder.Logging.AddSerilog() |> ignore
    builder.Services.AddTransient<Microsoft.Extensions.Logging.ILogger>(fun provider ->
        let loggerFactory = provider.GetRequiredService<ILoggerFactory>()
        loggerFactory.CreateLogger("Page")
    ) |> ignore

    // register data access
    builder.Services
        .AddScoped<IDbConnection>(fun sp ->
            let configuration = sp.GetRequiredService<IConfiguration>()
            new SqlConnection(configuration.GetConnectionString("SqlDatabase")))
        .AddScoped<IOutboxService, OutboxService>()
    |> ignore

    // register other stuff
    builder.Services.AddCors()    |> ignore
    builder.Services.AddGiraffe() |> ignore

    // setup application
    let app = builder.Build()
    app.UseHttpLogging() |> ignore

    if app.Environment.IsDevelopment() then
        app .UseDeveloperExceptionPage()
        |> ignore
    else
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection()
        |> ignore

    // TODO: add authentication / authorization

    app .UseStaticFiles()
        .UseGiraffe(webApp)

    Log.Logger.Information("Application Starting...")
    app.Run()

    0 // Exit code
