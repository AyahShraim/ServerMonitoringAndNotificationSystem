using Newtonsoft.Json;
using ConsumerProject.models.ConfigModels;
using ConsumerProject.Interfaces;
using ConsumerProject.models;
using ConsumerProject.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerStatisticsCollection.Models.ConfigModels;
using Microsoft.AspNetCore.SignalR.Client;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings/appsettings.json")
    .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IConfiguration>(configuration);
serviceCollection.Configure<RabbitMqConfiguration>(options => configuration.GetSection("RabbitMqSettings").Bind(options));
serviceCollection.Configure<MongoDbConfig>(options => configuration.GetSection("MongoDBConfig").Bind(options));
serviceCollection.Configure<AnomalyDetectionConfig>(options => configuration.GetSection("AnomalyDetectionConfig"));

serviceCollection.Configure<AnomalyDetectionConfig>(options => configuration.GetSection("AnomalyDetectionConfig").Bind(options));
serviceCollection.AddTransient<AnomalyDetectionService>();

serviceCollection.AddSingleton<IMessageQueueClientConsumer, RabbitMqMessageConsumer>();
var signalRConfig = configuration.GetSection("SignalRConfig").Get<SignalRConfig>();

serviceCollection.AddScoped<IDbService, MongoDbService>();
var hubConnection = new HubConnectionBuilder()
    .WithUrl(signalRConfig.SignalRUrl)
    .Build();

serviceCollection.AddSingleton(hubConnection);

var serviceProvider = serviceCollection.BuildServiceProvider();
var messageConsumer = serviceProvider.GetRequiredService<IMessageQueueClientConsumer>();
await messageConsumer.ConsumeAsync<ServerStatistics>("ServerStatistics.*", ProcessServerStatisticsMessage);

void ProcessServerStatisticsMessage(ServerStatistics serverStatistics)
{
    // Your logic to process the received server statistics message
    Console.WriteLine($"Received Server Statistics: {JsonConvert.SerializeObject(serverStatistics)}");
}
