using Microsoft.AspNetCore.SignalR;
using SignalRMonitor.Interfaces;

namespace SignalRMonitor.Hubs
{
    public sealed class SignalRAlertHub : Hub<IAlertService>
    {
        private readonly ILogger<SignalRAlertHub> _logger;
        public SignalRAlertHub(ILogger<SignalRAlertHub> logger)
        {
            _logger = logger;
        }

        public async Task SendAnomalyAlertAsync(string message)
        {
            _logger.LogInformation($"Alert Hub Received Anomaly Alert: {message}");
            await Clients.All.ReceiveAnomalyAlertAsync($"{Context.ConnectionId} : {message} ");
        }

        public async Task SendHighUsageAlertAsync(string message)
        {
            _logger.LogInformation($"Alert Hub Received High usage Alert: {message}");
            await Clients.All.ReceiveHighUsageAlertAsync($"{Context.ConnectionId} : {message} ");
        }
    }
}
