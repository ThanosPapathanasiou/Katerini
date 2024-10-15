using System;
using System.Data;
using Katerini.Core.Messaging;
using Katerini.Core.Outbox;
using Katerini.Service.Workers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Katerini.Service;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // setup logging 
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Seq(serverUrl: builder.Configuration["SeqLoggingConfiguration:ServerUrl"]!)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .CreateLogger();
        builder.Logging.AddSerilog();
        Serilog.Debugging.SelfLog.Enable(Console.Error);

        // setup RabbitMQ
        builder.Services.AddScoped<RabbitMqSettings>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            return new RabbitMqSettings("Katerini", configuration.GetConnectionString("RabbitMq")!);
        });
        builder.Services.AddScoped<IQueue, RabbitMqQueue>();

        // setup Database access
        builder.Services.AddScoped<IDbConnection>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            return new SqlConnection(configuration.GetConnectionString("SqlDatabase")!);
        });
        builder.Services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        // register message handlers (mediatr)
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMessage).Assembly));

        // each worker should create its own scope.
        builder.Services.AddHostedService<OutboxProcessorWorker>();
        builder.Services.AddHostedService<QueueProcessorWorker>();

        var host = builder.Build();

        Log.Logger.Information("Application Starting...");
        host.Run();
    }
}

