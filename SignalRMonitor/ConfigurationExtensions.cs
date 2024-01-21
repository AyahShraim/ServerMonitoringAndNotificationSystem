using SignalRMonitor.Models;

namespace SignalRMonitor
{
    public static class ConfigurationExtensions
    {
            public static SignalRConfig GetSignalRConfig(this IConfiguration configuration)
            {
                var signalRConfig = new SignalRConfig();
                configuration.GetSection("SignalRConfig").Bind(signalRConfig);
                return signalRConfig;
            }
    }
}
