using ConsumerProject.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace SignalRMonitor
{
    public sealed class SignalRAlertHub : Hub<IAlertService>
    {

        public override async Task OnConnectedAsync()
        {
            // This method is called when a connection is established
            Console.WriteLine($"Connection established: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public async Task SendAnomalyAlertAsync(string message)
        {
            Console.WriteLine(message);
            await Clients.All.ReceiveAnomalyAlertAsync($"{Context.ConnectionId} : {message} ");
        }

        public async Task SendHighUsageAlertAsync(string message)
        {
            Console.WriteLine(message);
            await Clients.All.ReceiveHighUsageAlertAsync($"{Context.ConnectionId} : {message} ");
        }
    }
}
