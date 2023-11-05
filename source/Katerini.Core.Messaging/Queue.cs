using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Katerini.Core.Messaging;

public interface IQueue
{
    public const string QueueName = "Katerini";
    
    // TODO: make enqueue idempotent, i.e. sending the same message twice should ignore the 2nd try.
    public void Enqueue(IMessage message);

    public Task<IMessage> DequeueAsync();
}

public class RabbitMqConnection : Uri
{
    public RabbitMqConnection(string uriString) : base(uriString)
    {
    }
}

public class RabbitMqQueue : IQueue
{
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqQueue(RabbitMqConnection rabbitMqUri)
    {
        _connectionFactory = new ConnectionFactory
        {
            Uri = rabbitMqUri
        };
    }

    public void Enqueue(IMessage message)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: IQueue.QueueName,
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
            routingKey: IQueue.QueueName,
            body: binaryMessage,
            basicProperties: properties);
    }

    public async Task<IMessage> DequeueAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: IQueue.QueueName,
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

        channel.BasicConsume(queue: IQueue.QueueName, autoAck: true, consumer: consumer);

        return await tcs.Task;
    }
}