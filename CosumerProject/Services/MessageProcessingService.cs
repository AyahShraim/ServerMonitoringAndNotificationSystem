using ConsumerProject.Interfaces;
using ConsumerProject.models;

namespace ConsumerProject.Services
{
    public class MessageProcessingService
    {
        private readonly IMessageQueueClientConsumer _messageConsumer;
        private readonly IDbService<ServerStatistics> _mongoDbService;
        private readonly AnomalyDetectionService _anomalyDetectionService;

        public MessageProcessingService(IMessageQueueClientConsumer messageConsumer, IDbService<ServerStatistics> mongoDbService, AnomalyDetectionService anomalyDetectionService)
        {
            _messageConsumer = messageConsumer;
            _anomalyDetectionService = anomalyDetectionService;
            _mongoDbService = mongoDbService;
        }

        public async Task StartProcessingAsync(string topicToConsumeFrom)
        {
            await _messageConsumer.ConsumeAsync<ServerStatistics>(topicToConsumeFrom, ProcessServerStatistics);
        }

        private void ProcessServerStatistics(ServerStatistics serverStatistics)
        {
             _mongoDbService.InsertOneAsync(serverStatistics).Wait(); 
             _anomalyDetectionService.CheckForAnomalies(serverStatistics).Wait();
        }
    }
}
