using Gateway.Models;
using Mongo2Go;
using MongoDB.Driver;

namespace Gateway.Data
{
    public class TestDatabase : IDatabase
    {
        private MongoDbRunner _runner;
        private IMongoClient _client;
        private IMongoDatabase _database;

        public TestDatabase()
        {
            _runner = MongoDbRunner.StartForDebugging();
            _client = new MongoClient(_runner.ConnectionString);
            _client.DropDatabase("envue");
            _database = _client.GetDatabase("envue");
            Seed.Run(this);
        }

        public IMongoCollection<T> GetCollection<T>() where T : IEntity
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }

        protected void Application_End()
        {
            _runner.Dispose();
        }


    }
}