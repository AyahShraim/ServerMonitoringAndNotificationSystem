using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerStatisticsCollection.Publishing.Models.ConfigModels;
using Newtonsoft.Json;
using System.Configuration;
using ServerStatisticsCollection.Models.ConfigModels;
using ConsumerProject.Interfaces;
using ConsumerProject.models;
using ConsumerProject.Services;
using ConsumerProject.Interfaces;
using ConsumerProject.models.ConfigModels;
using ConsumerProject.Services;


var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IConfiguration>(configuration);
serviceCollection.Configure<RabbitMqConfiguration>(options => configuration.GetSection("RabbitMqSettings").Bind(options));
serviceCollection.Configure<MongoDbConfig>(options => configuration.GetSection("MongoDBConfig").Bind(options));
serviceCollection.Configure<AnomalyDetectionConfig>(options => configuration.GetSection("AnomalyDetectionConfig"));

serviceCollection.AddSingleton<IMessageQueueClientConsumer, RabbitMqMessageConsumer>();
serviceCollection.AddScoped<IDbService, MongoDbService>(); 

var serviceProvider = serviceCollection.BuildServiceProvider();
var mongoDbConfig = configuration.GetSection("MongoDbConfig").Get<MongoDbConfig>();
Console.WriteLine($"ConnectionString: {mongoDbConfig.ConnectionString}, DatabaseName: {mongoDbConfig.DatabaseName}");





var messageConsumer = serviceProvider.GetRequiredService<IMessageQueueClientConsumer>();
await messageConsumer.ConsumeAsync<ServerStatistics>("ServerStatistics.*", ProcessServerStatisticsMessage);

void ProcessServerStatisticsMessage(ServerStatistics serverStatistics)
{
    // Your logic to process the received server statistics message
    Console.WriteLine($"Received Server Statistics: {JsonConvert.SerializeObject(serverStatistics)}");
}