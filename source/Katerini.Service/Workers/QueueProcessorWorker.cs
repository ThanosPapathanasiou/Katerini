using System;
using System.Threading;
using System.Threading.Tasks;
using Katerini.Core.Messaging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Katerini.Service.Workers;

public class QueueProcessorWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<QueueProcessorWorker> _logger;

    public QueueProcessorWorker(IServiceProvider services, ILogger<QueueProcessorWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Worker} started at: {Time}", nameof(QueueProcessorWorker), DateTimeOffset.UtcNow);
        using var scope = _services.CreateScope();
        {
            var queue = scope.ServiceProvider.GetRequiredService<IQueue>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("{Worker} running at: {Time}", nameof(QueueProcessorWorker), DateTimeOffset.UtcNow);
                var message = await queue.DequeueAsync();
                await mediator.Send(message, stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}