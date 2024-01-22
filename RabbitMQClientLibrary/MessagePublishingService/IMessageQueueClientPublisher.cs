namespace RabbitMQClientLibrary.MessagePublishingService
{
    public interface IMessageQueueClientPublisher
    {
        Task PublishAsync<T>(string topic, T message, string exchangeName);
    }
}
