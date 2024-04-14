using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Katerini.Core.Messaging;

namespace Katerini.Core.Outbox;

public interface IOutboxService
{
    Task AddToOutboxAsync(IMessage message);
}

public class OutboxService(IDbConnection connection) : IOutboxService
{
    public async Task AddToOutboxAsync(IMessage message)
    {
        const string sql = """
                           INSERT INTO OutboxMessages (Id, MessageType, Payload)
                           VALUES (@Id, @MessageType, @Payload);
                           """;

        var outboxMessage = new OutboxMessage
        {
            Id = message.MessageId,
            MessageType = message.GetType().Name,
            Payload = JsonSerializer.Serialize(message, message.GetType())
        };

        await connection.ExecuteAsync(sql, outboxMessage);
    }
}