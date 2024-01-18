using ConsumerProject.models;
using ConsumerProject.models.ConfigModels;
using Microsoft.Extensions.Options;


namespace ConsumerProject.Services
{
    public class MessageProcessingService
    {
        private readonly AnomalyDetectionConfig _anomalyDetectionConfig;
        private ServerStatistics _previousServerStatistics;

        public MessageProcessingService(IOptions<AnomalyDetectionConfig> anomalyDetectionConfig)
        {
            _anomalyDetectionConfig = anomalyDetectionConfig.Value;
            _previousServerStatistics = new ServerStatistics();
        }
        private void CheckForAnomalies(ServerStatistics currentServerStatistics)
        {
            double memoryUsageThresholdPercentage = _anomalyDetectionConfig.MemoryUsageThresholdPercentage;
            double memoryUsageAnomalyThresholdPercentage = _anomalyDetectionConfig.MemoryUsageThresholdPercentage;
            double cpuUsageThresholdPercentage = _anomalyDetectionConfig.CpuUsageThresholdPercentage;
            double cpuUsageAnomalyThresholdPercentage = _anomalyDetectionConfig.CpuUsageAnomalyThresholdPercentage;
            DetectHighCpuUsage(currentServerStatistics.CpuUsage, cpuUsageThresholdPercentage);
            DetectHighMemoryUsage(currentServerStatistics.MemoryUsage, currentServerStatistics.AvailableMemory, memoryUsageThresholdPercentage);
            DetectAnomalyCpuUsage(currentServerStatistics.CpuUsage, _previousServerStatistics.CpuUsage, cpuUsageAnomalyThresholdPercentage);
            DetectAnomalyMemoryUsage(currentServerStatistics.MemoryUsage, _previousServerStatistics.MemoryUsage, memoryUsageAnomalyThresholdPercentage);
        }

        private bool DetectAnomalyMemoryUsage(double currentMemoryUsage, double previousMemoryUsage, double thresholdPercentage)
        {
            return currentMemoryUsage > previousMemoryUsage * (1 + thresholdPercentage);
        }
        private bool DetectAnomalyCpuUsage(double currentCpuUsage, double previousCpuUsage, double thresholdPercentage)
        {
            return currentCpuUsage > previousCpuUsage * (1 + thresholdPercentage);
        }
        private bool DetectHighMemoryUsage(double currentMemoryUsage, double currentAvailableMemory, double thresholdPercentage)
        {
            return currentMemoryUsage / (currentMemoryUsage + currentAvailableMemory) > thresholdPercentage;

        }
        private bool DetectHighCpuUsage(double currentCpuUsage, double thresholdPercentage)
        {
            return currentCpuUsage > thresholdPercentage;
        }
    }
}
