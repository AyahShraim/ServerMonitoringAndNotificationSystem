using Microsoft.Extensions.Configuration;

namespace SignalREventConsumerService.Extensions;

public static class SignalRConfigHelper
{
    public static IConfiguration GetConfiguration()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile("appSettings.json")
             .Build();
            return configuration;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error loading configuration from appSettings.json", ex);
        }
    }
}
