using RabbitMQ.Client;

namespace ConsumerProject.Interfaces
{
    public interface IMessageQueueClientConsumer
    {
        Task ConsumeAsync<T>(string topic, Action<T> onMessageReceived);
    }
}
