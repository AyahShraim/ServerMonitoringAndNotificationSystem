using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ConsumerProject.models;
using ConsumerProject.Interfaces;
using ConsumerProject.models.ConfigModels;
namespace ConsumerProject.Services
{
    public class MongoDbService : IDbService
    {
        private readonly IMongoCollection<ServerStatistics> _collection;

        public MongoDbService(IOptions<MongoDbConfig> mongoDbConfig)
        {
            var config = mongoDbConfig.Value;
            var client = new MongoClient(config.ConnectionString);
            var database = client.GetDatabase(config.DatabaseName);
            _collection = database.GetCollection<ServerStatistics>("ServerStatistics");
        }
        public void InsertOne(ServerStatistics serverStatistics)
        {
            _collection.InsertOne(serverStatistics);
        }
    }
}

