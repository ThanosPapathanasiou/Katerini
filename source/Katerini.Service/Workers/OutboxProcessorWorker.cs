using System;
using System.Threading;
using System.Threading.Tasks;
using Katerini.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Katerini.Service.Workers;

public class OutboxProcessorWorker(IServiceProvider services, ILogger<OutboxProcessorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Worker} started at: {Time}", nameof(OutboxProcessorWorker), DateTimeOffset.UtcNow);
        using var scope = services.CreateScope();
        var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("{Worker} running at: {Time}", nameof(OutboxProcessorWorker), DateTimeOffset.UtcNow);
            await outboxProcessor.ProcessOutboxMessagesAsync(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}