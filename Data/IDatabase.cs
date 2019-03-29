using Gateway.Models;
using MongoDB.Driver;

namespace Gateway.Data
{
    public interface IDatabase
    {
        IMongoCollection<T> GetCollection<T>() where T : IEntity;
    }
}