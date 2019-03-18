using Gateway.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Gateway.Data
{
    public class EnvueData
    {
        private readonly IMongoDatabase _database = null;

        public EnvueData(IConfiguration configuration)
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

        public IMongoCollection<User> Users
        {
            get
            {
                return _database.GetCollection<User>("User");
            }
        }

    }
}