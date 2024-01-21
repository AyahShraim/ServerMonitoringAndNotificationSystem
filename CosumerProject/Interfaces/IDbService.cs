using ConsumerProject.models;

namespace ConsumerProject.Interfaces
{
    public interface IDbService
    {
        Task InsertOneAsync(ServerStatistics serverStatistics);
    }
}
