using ConsumerProject.models;

namespace ConsumerProject.Interfaces
{
    public interface IDbService
    {
        void InsertOne(ServerStatistics serverStatistics);
    }
}
