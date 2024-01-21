using ConsumerProject.models;
using ConsumerProject.models.ConfigModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace ConsumerProject.Services
{
    public class AnomalyDetectionService
    {
        private readonly AnomalyDetectionConfig _anomalyDetectionConfig;
        private  ServerStatistics _previousServerStatistics;
        private readonly HubConnection _hubConnection;

        public AnomalyDetectionService(IOptions<AnomalyDetectionConfig> anomalyDetectionConfig, HubConnection hubConnection)
        {
            _anomalyDetectionConfig = anomalyDetectionConfig.Value;
            _previousServerStatistics = new ServerStatistics();
            _hubConnection = hubConnection;
        }
      
        public async Task  CheckForAnomalies(ServerStatistics currentServerStatistics)
        {
            double memoryUsageThresholdPercentage = _anomalyDetectionConfig.MemoryUsageThresholdPercentage;
            double memoryUsageAnomalyThresholdPercentage = _anomalyDetectionConfig.MemoryUsageThresholdPercentage;
            double cpuUsageThresholdPercentage = _anomalyDetectionConfig.CpuUsageThresholdPercentage;
            double cpuUsageAnomalyThresholdPercentage = _anomalyDetectionConfig.CpuUsageAnomalyThresholdPercentage;

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
            await DetectHighCpuUsage(currentServerStatistics.CpuUsage, cpuUsageThresholdPercentage);
            await DetectHighMemoryUsage(currentServerStatistics.MemoryUsage, currentServerStatistics.AvailableMemory, memoryUsageThresholdPercentage);
            await DetectAnomalyCpuUsage(currentServerStatistics.CpuUsage, _previousServerStatistics.CpuUsage, cpuUsageAnomalyThresholdPercentage);
            await DetectAnomalyMemoryUsage(currentServerStatistics.MemoryUsage, _previousServerStatistics.MemoryUsage, memoryUsageAnomalyThresholdPercentage);
            await _hubConnection.StopAsync();
        }

        private async Task DetectAnomalyMemoryUsage(double currentMemoryUsage, double previousMemoryUsage, double thresholdPercentage)
        {
            bool isAnomalyMemoryUsage =  currentMemoryUsage > previousMemoryUsage * (1 + thresholdPercentage);
            if (isAnomalyMemoryUsage)
               await SendAnomalyUsageAlert("Anomaly memory usage detected !");
        }

        private async Task SendAnomalyUsageAlert(string msg)
        {
            await _hubConnection.SendAsync("SendAnomalyAlertAsync", msg);
        }
        
        private async Task DetectAnomalyCpuUsage(double currentCpuUsage, double previousCpuUsage, double thresholdPercentage)
        {

            bool isAnomalyCpuUsage =  currentCpuUsage > previousCpuUsage * (1 + thresholdPercentage);
            if (isAnomalyCpuUsage)
            {
                await SendAnomalyUsageAlert("Anomaly cpu usage detected !");
            }         
        }
        
        private async Task DetectHighMemoryUsage(double currentMemoryUsage, double currentAvailableMemory, double thresholdPercentage)
        {
            bool isHighMemoryUsage = currentMemoryUsage / (currentMemoryUsage + currentAvailableMemory) > thresholdPercentage;
            if (isHighMemoryUsage)
                await SendHighUsageAlert("High memory usage detected !");
        }
        
        private async Task SendHighUsageAlert(string msg)
        {
            await _hubConnection.SendAsync("SendHighUsageAlertAsync", msg);
        }
        
        private async Task  DetectHighCpuUsage(double currentCpuUsage, double thresholdPercentage)
        {
            bool isHighCpuUsage =  currentCpuUsage > thresholdPercentage;
            if (isHighCpuUsage)
                await SendHighUsageAlert("High cpu usage detected !");
        }
    }
}
