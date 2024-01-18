using Microsoft.Extensions.Options;
using ServerStatisticsCollection.Models;
using ServerStatisticsCollection.Models.ConfigModels;
using System.Diagnostics;
using task1.Interfaces;
namespace ServerStatisticsCollection.Services
{
    public class ServerStatisticsCollector
    {
        private ServerStatisticsConfig _serverStatisticsConfig;
        private readonly IMessageQueueClientPublisher _messageQueueClient;

        public ServerStatisticsCollector(IMessageQueueClientPublisher messageQueueClient, IOptions<ServerStatisticsConfig> serverStatisticsConfig)
        {
            _serverStatisticsConfig = serverStatisticsConfig.Value;
            _messageQueueClient = messageQueueClient;
        }

        private ServerStatistics CollectServerStatistics()
        {
            var memoryUsage = GetMemoryUsage();
            var availableMemory = GetAvailableMemory();
            var cpuUsage = GetCpuUsage();

            return new ServerStatistics
            {
                MemoryUsage = memoryUsage,
                AvailableMemory = availableMemory,
                CpuUsage = cpuUsage,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task CollectServerStatisticsPeriodically()
        {
            int timeInterval = _serverStatisticsConfig.SamplingIntervalSeconds;
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(timeInterval * 1000));
            while (await timer.WaitForNextTickAsync())
            {
                var statistics = CollectServerStatistics();
                await _messageQueueClient.PublishAsync($"ServerStatistics.{_serverStatisticsConfig.ServerIdentifier}", statistics);
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

