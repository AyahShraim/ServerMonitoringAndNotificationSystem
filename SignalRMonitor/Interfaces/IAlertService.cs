namespace SignalRMonitor.Interfaces
{
    public interface IAlertService
    {
        Task ReceiveAnomalyAlertAsync(string message);

        Task ReceiveHighUsageAlertAsync(string message);
    }
}
