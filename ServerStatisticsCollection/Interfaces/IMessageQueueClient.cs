using RabbitMQ.Client;

namespace ServerStatisticsCollection.Interfaces
{
    public interface IMessageQueueClientPublisher
    {
        IConnection CreateConnection();
        Task PublishAsync<T>(string topic, T message);    
    }
}
