using ConsumerProject.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQClientLibrary.Configuration;
using System.Text;
using System.Text.Json;

namespace ConsumerProject.Services
{
    public class RabbitMqMessageConsumer : IMessageQueueClientConsumer
    {
        private readonly RabbitMQConfiguration _rabbitMqConfig;
        private readonly ILogger<RabbitMqMessageConsumer> _logger;

        public RabbitMqMessageConsumer(IOptions<RabbitMQConfiguration> rabbitMqConfig, ILogger<RabbitMqMessageConsumer> logger)
        {
            _rabbitMqConfig = rabbitMqConfig.Value;
            _logger = logger;
        }

        public async Task ConsumeAsync<T>(string topic, Action<T> onMessageReceived)
        {
            try
            {
                using var connection = CreateConnection();
                using var channel = connection.CreateModel();
                var exchangeName = "server_statistics_exchange";
                var queueName = channel.QueueDeclare().QueueName;
                channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                channel.QueueBind(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: topic
                );
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, args) =>
                {
                    var body = args.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var deserializedMessage = JsonSerializer.Deserialize<T>(message);
                    onMessageReceived?.Invoke(deserializedMessage);
                    _logger.LogInformation($"Successfully consumed message from RabbitMQ. Topic: {topic}");
                    await Task.CompletedTask;
                    channel.BasicAck(args.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error consuming message from RabbitMQ. Topic: {topic}");
            }

        }

        private IConnection CreateConnection()
        {
            try
            {
                ConnectionFactory connectionFactory = new()
                {
                    UserName = _rabbitMqConfig.Username,
                    Password = _rabbitMqConfig.Password,
                    HostName = _rabbitMqConfig.HostName,
                    DispatchConsumersAsync = true
                };
                return connectionFactory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RabbitMQ connection.");
                throw;
            }
        }
    }
}
