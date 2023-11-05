using System;
using System.Threading;
using System.Threading.Tasks;
using Katerini.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Katerini.Service.Workers;

public class OutboxProcessorWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OutboxProcessorWorker> _logger;

    public OutboxProcessorWorker(IServiceProvider services, ILogger<OutboxProcessorWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Worker} started at: {Time}", nameof(OutboxProcessorWorker), DateTimeOffset.UtcNow);
        using var scope = _services.CreateScope();
        {
            var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("{Worker} running at: {Time}", nameof(OutboxProcessorWorker), DateTimeOffset.UtcNow);
                await outboxProcessor.ProcessOutboxMessagesAsync(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}