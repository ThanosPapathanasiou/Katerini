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

public class ContactUsMessageHandler(ILogger<ContactUsMessageHandler> logger) : IRequestHandler<ContactUsMessage>
{
    public Task Handle(ContactUsMessage request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message '{MessageType}' with id: '{MessageId}'", request.GetType().Name, request.MessageId);
        return Task.CompletedTask;
    }
}