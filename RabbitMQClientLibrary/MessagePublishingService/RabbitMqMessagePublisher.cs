using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQClientLibrary.Configuration;
using RabbitMQClientLibrary.MessagePublishingService;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServerStatisticsCollection.Services
{
    public class RabbitMqMessagePublisher : IMessageQueueClientPublisher
    {
        private readonly RabbitMQConfiguration _configuration;
        private readonly string _defaultExchangeName = "server_statistics_exchange";
        private readonly ILogger<RabbitMqMessagePublisher> _logger;
        public RabbitMqMessagePublisher(IOptions<RabbitMQConfiguration> rabbitMqConfig, ILogger<RabbitMqMessagePublisher> logger)
        {
            _configuration = rabbitMqConfig.Value;
            _logger = logger;
        }

        public async Task PublishAsync<T>(string topic, T message, string? exchangeName = null)
        {
            try
            {
                using var connection = CreateConnection();
                using var channel = connection.CreateModel();
                var actualExchangeName = exchangeName ?? _defaultExchangeName;
                channel.ExchangeDeclare(actualExchangeName, ExchangeType.Topic);
                var jsonMessage = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(jsonMessage);
                await Task.Run(() => channel.BasicPublish(exchange: actualExchangeName, routingKey: topic, basicProperties: null, body: body));
                _logger.LogInformation($"Successfully published message to RabbitMQ with topic: {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing message to RabbitMQ. Exchange: {exchangeName}, Topic: {topic}");
            }
        }

        private IConnection CreateConnection()
        {
            try
            { 
                ConnectionFactory connectionFactory = new()
                {
                    UserName = _configuration.Username,
                    Password = _configuration.Password,
                    HostName = _configuration.HostName,
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
