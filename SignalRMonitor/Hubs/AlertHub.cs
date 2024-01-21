using Microsoft.AspNetCore.SignalR;
using SignalRMonitor.Interfaces;

namespace SignalRMonitor.Hubs
{
    public sealed class SignalRAlertHub : Hub<IAlertService>
    {
        public async Task SendAnomalyAlertAsync(string message)
        {
            await Clients.All.ReceiveAnomalyAlertAsync($"{Context.ConnectionId} : {message} ");
        }

        public async Task SendHighUsageAlertAsync(string message)
        {
            await Clients.All.ReceiveHighUsageAlertAsync($"{Context.ConnectionId} : {message} ");
        }
    }
}
