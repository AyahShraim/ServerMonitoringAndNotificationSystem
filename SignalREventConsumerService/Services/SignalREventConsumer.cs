using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace SignalREventConsumerService.Services
{
    public class SignalREventConsumer : IDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger _logger;

        public SignalREventConsumer(string HubURL , ILogger logger)
        {
            _logger = logger;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(HubURL)
                .Build();
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation($"Connected to SignalR hub at {_hubConnection.ConnectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error connecting to SignalR hub: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            await _hubConnection.StopAsync();
            _logger.LogInformation("SignalR connection stopped.");
        }

        public void SubscribeToEvents()
        {
            _hubConnection.On<string>("ReceiveAnomalyAlertAsync", (message) =>
            {
                _logger.LogInformation($"Received Anomaly Alert: {message}");
            });

            _hubConnection.On<string>("ReceiveHighUsageAlertAsync", (message) =>
            {
                _logger.LogInformation($"Received High Usage Alert: {message}");
            });
        }

        public void Dispose()
        {
            _hubConnection.DisposeAsync();
        }
    }

}
