using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Katerini.Core.Messaging;

public interface IQueue
{
    // TODO: make enqueue idempotent, i.e. sending the same message twice should ignore the 2nd try.
    public void Enqueue(IMessage message);

    public Task<IMessage> DequeueAsync();
}

public record RabbitMqSettings(string QueueName, string ConnectionString);

public class RabbitMqQueue(RabbitMqSettings settings) : IQueue
{
    private readonly string _queueName = settings.QueueName;
    private readonly ConnectionFactory _connectionFactory = new()
    {
        Uri = new Uri(settings.ConnectionString)
    };

    public void Enqueue(IMessage message)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var properties = channel.CreateBasicProperties();
        properties.CorrelationId = message.MessageId.ToString();
        properties.MessageId = message.MessageId.ToString();
        properties.ContentType = "application/json";
        properties.Type = message.GetType().Name;

        var jsonMessage = JsonSerializer.Serialize(message, message.GetType());
        var binaryMessage = Encoding.UTF8.GetBytes(jsonMessage);

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: _queueName,
            body: binaryMessage,
            basicProperties: properties);
    }

    public async Task<IMessage> DequeueAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var tcs = new TaskCompletionSource<IMessage>();

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, ea) =>
        {
            var messageType = ea.BasicProperties.Type; // this is: 'message.GetType().Name'
            var binaryMessage = ea.Body.ToArray();
            var receivedMessage = MessagingHelpers.ConvertToIMessage(binaryMessage, messageType);
            tcs.SetResult(receivedMessage);
        };

        channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        return await tcs.Task;
    }
}

// TODO: support more queue technologies like Azure service bus