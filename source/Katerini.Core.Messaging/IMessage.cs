using System;
using MediatR;

namespace Katerini.Core.Messaging;

public interface IMessage : IRequest
{
    Guid MessageId { get; init; }
}