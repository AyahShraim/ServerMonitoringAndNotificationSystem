using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQClientLibrary.Configuration;
using RabbitMQClientLibrary.MessagePublishingService;
using ServerStatisticsCollection.Models.ConfigModels;
using ServerStatisticsCollection.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings/appsettings.json")
    .Build();

var serviceCollection = new ServiceCollection();

serviceCollection.AddLogging(builder =>
{
    builder.AddConsole(); 
});

serviceCollection.AddSingleton<IConfiguration>(configuration);
serviceCollection.Configure<ServerStatisticsConfig>(options => configuration.GetSection("ServerStatisticsConfig").Bind(options));
serviceCollection.Configure<RabbitMQConfiguration>(options => configuration.GetSection("RabbitMqSettings").Bind(options));
serviceCollection.AddSingleton<IMessageQueueClientPublisher, RabbitMqMessagePublisher>();
serviceCollection.AddTransient<ServerStatisticsCollector>();


using var serviceProvider = serviceCollection.BuildServiceProvider();
var serverStatisticsCollector = serviceProvider.GetRequiredService<ServerStatisticsCollector>();

await serverStatisticsCollector.CollectServerStatisticsPeriodically();

