using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway
{
    public class Data
    {
        public IMongoDatabase Database { get; }

        public Data(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Envue");
            var dbname = MongoUrl.Create(connectionString).DatabaseName;
            var client = new MongoClient(connectionString);
            if (client != null)
            {
                Database = client.GetDatabase(dbname);
            }
            else
            {
                throw new MongoClientException("MongoDB client not initialised");
            }
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }
    }
}