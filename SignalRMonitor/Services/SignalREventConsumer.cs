using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRMonitor.Services
{
    public class SignalREventConsumer
    {
        private readonly HubConnection _hubConnection;

        private SignalREventConsumer(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;
        }

        public static SignalREventConsumer Create(IConfiguration configuration)
        {
            var signalRConfig = configuration.GetSignalRConfig();
            var hubConnection = new HubConnectionBuilder()
                .WithUrl(signalRConfig.SignalRUrl)
                .Build();

            return new SignalREventConsumer(hubConnection);
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine($"Connected to SignalR hub at {_hubConnection.ConnectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to SignalR hub: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            await _hubConnection.StopAsync();
            Console.WriteLine("SignalR connection stopped.");
        }

        public void SubscribeToEvents()
        {

            _hubConnection.On<string>("ReceiveAnomalyAlertAsync", (message) =>
            {
                Console.WriteLine($"Received Anomaly Alert: {message}");
            });

            _hubConnection.On<string>("ReceiveHighUsageAlertAsync", (message) =>
            {
                Console.WriteLine($"Received High Usage Alert: {message}");
            });
        }

    }
}
