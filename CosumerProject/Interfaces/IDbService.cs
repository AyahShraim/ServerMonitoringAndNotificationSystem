using ConsumerProject.models;

namespace ConsumerProject.Interfaces
{
    public interface IDbService<T>
    {
        Task InsertOneAsync(T data);
    }
}
