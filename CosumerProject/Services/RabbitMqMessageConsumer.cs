using ConsumerProject.Interfaces;
using ConsumerProject.models;
using ConsumerProject.Interfaces;
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
        public RabbitMqMessageConsumer(IOptions<RabbitMqConfiguration> rabbitMqConfig, IDbService mongoDbService)
        {
            _rabbitMqConfig = rabbitMqConfig.Value;
            _mongoDbService = mongoDbService;
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
                PersistToMongoDB(deserializedMessage);
                onMessageReceived?.Invoke(deserializedMessage);
                await Task.CompletedTask;
                channel.BasicAck(args.DeliveryTag, multiple: false);
                Console.WriteLine(deserializedMessage.ToString());
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            Console.ReadLine();
        }

        private void PersistToMongoDB<T>(T deserializedMessage)
        {
            if (deserializedMessage is ServerStatistics serverStatistics)
            {
                _mongoDbService.InsertOne(serverStatistics);
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
