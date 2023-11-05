using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Katerini.Core.Messaging;

namespace Katerini.Core.Outbox;

public interface IOutboxProcessor
{
    Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken);
}

public class OutboxProcessor : IOutboxProcessor
{
    private readonly IQueue _queue;
    private readonly IDbConnection _connection;

    public OutboxProcessor(IQueue queue, IDbConnection connection)
    {
        _queue = queue;
        _connection = connection;
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        const string getUnprocessedMessages = "SELECT * FROM OutboxMessages WHERE ProcessedAt is NULL";
        const string updateProcessedMessage = "UPDATE OutboxMessages SET ProcessedAt = GETUTCDATE() WHERE Id = @Id";

        var unprocessedOutboxMessages = _connection
            .Query<OutboxMessage>(getUnprocessedMessages)
            .ToList();

        foreach (var unprocessedOutboxMessage in unprocessedOutboxMessages)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            // Send the message
            var queueMessage = MessagingHelpers.ConvertToIMessage(
                jsonMessage: unprocessedOutboxMessage.Payload,
                messageType: unprocessedOutboxMessage.MessageType);
            _queue.Enqueue(queueMessage);

            // Mark as processed
            await _connection.ExecuteAsync(updateProcessedMessage, new { Id = unprocessedOutboxMessage.Id });
        }
    }
}