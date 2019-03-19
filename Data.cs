using Gateway.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Gateway
{
    public class Data
    {
        private readonly IMongoDatabase _database = null;

        public Data(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Envue");
            var dbname = MongoUrl.Create(connectionString).DatabaseName;
            var client = new MongoClient(connectionString);
            if (client != null)
            {
                _database = client.GetDatabase(dbname);
            }
            else
            {
                throw new MongoClientException("MongoDB client not initialised");
            }
        }

        public IMongoCollection<Account> Accounts => _database.GetCollection<Account>("Account");

    }
}