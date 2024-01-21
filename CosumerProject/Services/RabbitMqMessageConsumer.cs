using ConsumerProject.Interfaces;
using ConsumerProject.models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServerStatisticsCollection.Models.ConfigModels;
using System.Text;
using System.Text.Json;

namespace ConsumerProject.Services
{
    public class RabbitMqMessageConsumer : IMessageQueueClientConsumer
    {
        private readonly RabbitMqConfiguration _rabbitMqConfig;
        private readonly IDbService _mongoDbService;
        private readonly AnomalyDetectionService _anomalyDetectionService;

        public RabbitMqMessageConsumer(IOptions<RabbitMqConfiguration> rabbitMqConfig, IDbService mongoDbService, AnomalyDetectionService anomalyDetectionService)
        {
            _rabbitMqConfig = rabbitMqConfig.Value;
            _mongoDbService = mongoDbService;
            _anomalyDetectionService = anomalyDetectionService;
        }

        public async Task ConsumeAsync<T>(string topic, Action<T> onMessageReceived)
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
                await ProcessMessage(deserializedMessage);
                onMessageReceived?.Invoke(deserializedMessage);
                await Task.CompletedTask;
                channel.BasicAck(args.DeliveryTag, multiple: false);
            };

           channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            await Task.Delay(Timeout.Infinite);

        }

        private async Task ProcessMessage<T>(T deserializedMessage)
        {
            if (deserializedMessage is ServerStatistics serverStatistics)
            {
                await _mongoDbService.InsertOneAsync(serverStatistics);
                await _anomalyDetectionService.CheckForAnomalies(serverStatistics);
            }
            else
            {
                Console.WriteLine("Error: Received message is not of type ServerStatistics");
            }
        }

        public IConnection CreateConnection()
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

    }
}
