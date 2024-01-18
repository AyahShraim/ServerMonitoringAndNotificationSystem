using RabbitMQ.Client;

namespace ConsumerProject.Interfaces
{
    public interface IMessageQueueClientConsumer
    {
        IConnection CreateConnection();
        Task ConsumeAsync<T>(string topic, Action<T> onMessageReceived);
    }
}
