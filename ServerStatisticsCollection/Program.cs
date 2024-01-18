using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerStatisticsCollection.Models.ConfigModels;
using ServerStatisticsCollection.Services;
using ServerStatisticsCollection.Interfaces;


var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings/appsettings.json")
    .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IConfiguration>(configuration);

serviceCollection.Configure<ServerStatisticsConfig>(options => configuration.GetSection("ServerStatisticsConfig").Bind(options));
serviceCollection.Configure<RabbitMqConfiguration>(options => configuration.GetSection("RabbitMqSettings").Bind(options));


serviceCollection.AddSingleton<IMessageQueueClientPublisher, RabbitMqMessagePublisher>();
serviceCollection.AddTransient<ServerStatisticsCollector>();


var serviceProvider = serviceCollection.BuildServiceProvider();

var serverStatisticsCollector = serviceProvider.GetRequiredService<ServerStatisticsCollector>();

await serverStatisticsCollector.CollectServerStatisticsPeriodically();

