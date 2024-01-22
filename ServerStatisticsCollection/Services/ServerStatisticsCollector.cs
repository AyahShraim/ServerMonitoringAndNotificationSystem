using Microsoft.Extensions.Options;
using RabbitMQClientLibrary.MessagePublishingService;
using ServerStatisticsCollection.Models;
using ServerStatisticsCollection.Models.ConfigModels;
using System.Diagnostics;
namespace ServerStatisticsCollection.Services
{
    public class ServerStatisticsCollector
    {
        private readonly ServerStatisticsConfig _serverStatisticsConfig;
        private readonly IMessageQueueClientPublisher _messagePublisher;

        public ServerStatisticsCollector(IMessageQueueClientPublisher messagePublisher, IOptions<ServerStatisticsConfig> serverStatisticsConfig)
        {
            _serverStatisticsConfig = serverStatisticsConfig.Value;
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        }

        private ServerStatistics CollectServerStatistics()
        {
            var memoryUsage = GetMemoryUsage();
            var availableMemory = GetAvailableMemory();
            var cpuUsage = GetCpuUsage();

            return new ServerStatistics
            {
                ServerIdentifier = _serverStatisticsConfig.ServerIdentifier,
                MemoryUsage = memoryUsage,
                AvailableMemory = availableMemory,
                CpuUsage = cpuUsage,
                Timestamp = DateTime.Now
            };
        }

        public async Task CollectServerStatisticsPeriodically()
        {
            int timeInterval = _serverStatisticsConfig.SamplingIntervalSeconds;
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(timeInterval * 1000));
            while (await timer.WaitForNextTickAsync())
            {
                var statistics = CollectServerStatistics();
                await _messagePublisher.PublishAsync($"ServerStatistics.{_serverStatisticsConfig.ServerIdentifier}", statistics, null);
            }
        }

        private static double GetMemoryUsage()
        {
            using var usageMemoryCounter = new PerformanceCounter("Memory", "Committed Bytes");
            double committedMemoryBytes = usageMemoryCounter.NextValue();
            return committedMemoryBytes / (1024 * 1024);
        }

        private static double GetCpuUsage()
        {
            using PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            Thread.Sleep(1000);
            var cpuUsagePercentage = cpuCounter.NextValue();
            return cpuUsagePercentage / 100;
        }

        private static double GetAvailableMemory()
        {
            using var availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            return availableMemoryCounter.NextValue();
        }
    }
}

