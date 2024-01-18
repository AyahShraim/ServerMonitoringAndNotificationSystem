using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ServerStatisticsCollection.Models.ConfigModels;
using System.Text;
using ServerStatisticsCollection.Interfaces;

namespace ServerStatisticsCollection.Services
{
    public class RabbitMqMessagePublisher : IMessageQueueClientPublisher
    {

        private readonly RabbitMqConfiguration _rabbitMqConfig;
        public RabbitMqMessagePublisher(IOptions<RabbitMqConfiguration> rabbitMqConfig)
        {
            _rabbitMqConfig = rabbitMqConfig.Value;
        }

        public async Task PublishAsync<T>(string topic, T message)
        {
            using var connection = CreateConnection();
            using var channel = connection.CreateModel();
            var exchangeName = "server_statistics_exchange";
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
            var jsonMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            await Task.Run(() => channel.BasicPublish(exchange: "server_statistics_exchange", routingKey: topic, basicProperties: null, body: body));
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
