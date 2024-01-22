using ConsumerProject.models.ConfigModels;
using ConsumerProject.Interfaces;
using ConsumerProject.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using RabbitMQClientLibrary.Configuration;
using Microsoft.Extensions.Logging;

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

serviceCollection.Configure<RabbitMQConfiguration>(options => configuration.GetSection("RabbitMqSettings").Bind(options));
serviceCollection.Configure<MongoDbConfig>(options => configuration.GetSection("MongoDBConfig").Bind(options));
serviceCollection.Configure<AnomalyDetectionConfig>(options => configuration.GetSection("AnomalyDetectionConfig").Bind(options));

serviceCollection.AddScoped<IDbService, MongoDbService>();
serviceCollection.AddTransient<AnomalyDetectionService>();
serviceCollection.AddSingleton<IMessageQueueClientConsumer, RabbitMqMessageConsumer>();

var signalRConfig = configuration.GetSection("SignalRConfig").Get<SignalRConfig>();
var hubConnection = new HubConnectionBuilder()
    .WithUrl(signalRConfig.SignalRUrl)
    .Build();
serviceCollection.AddSingleton(hubConnection);

serviceCollection.AddTransient<MessageProcessingService>();

var serviceProvider = serviceCollection.BuildServiceProvider();
var messageProcessor = serviceProvider.GetRequiredService<MessageProcessingService>();
await messageProcessor.StartProcessingAsync("ServerStatistics.*");


