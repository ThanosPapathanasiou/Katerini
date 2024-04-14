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

public class OutboxProcessor(IQueue queue, IDbConnection connection) : IOutboxProcessor
{
    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        const string getUnprocessedMessages = "SELECT * FROM OutboxMessages WHERE ProcessedAt is NULL";
        const string updateProcessedMessage = "UPDATE OutboxMessages SET ProcessedAt = GETUTCDATE() WHERE Id = @Id";

        var unprocessedOutboxMessages = connection
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
            queue.Enqueue(queueMessage);

            // Mark as processed
            await connection.ExecuteAsync(updateProcessedMessage, new { Id = unprocessedOutboxMessage.Id });
        }
    }
}