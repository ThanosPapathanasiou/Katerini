using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katerini.Core.Messaging.Messages;

public record ContactUsMessage(string FirstName, string LastName, string Email) : IMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
}

public class ContactUsMessageHandler : IRequestHandler<ContactUsMessage>
{
    private readonly ILogger<ContactUsMessageHandler> _logger;

    public ContactUsMessageHandler(ILogger<ContactUsMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ContactUsMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message '{MessageType}' with id: '{MessageId}'", request.GetType().Name, request.MessageId);
        return Task.CompletedTask;
    }
}