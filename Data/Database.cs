using Gateway.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Gateway.Data
{
    public class Database : IDatabase
    {
        private IMongoClient _client;
        private IMongoDatabase _database;

        public Database(IConfiguration configuration)
        {
            _client = new MongoClient(configuration.GetConnectionString("Envue"));
            _database = _client.GetDatabase("envue");
        }

        public IMongoCollection<T> GetCollection<T>() where T : Entity
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }
    }
}