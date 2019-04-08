using System;
using System.Linq;
using System.Reflection;
using Gateway;
using Gateway.Models;
using Gateway.MongoDB.Repositories;
using Gateway.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Gateway.MongoDB.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the mongoDB repository dependency injection. 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString">If not specified, an in memory mongo database is used.</param>
        /// <returns></returns>
        public static IServiceCollection AddMongoDBRepositories(this IServiceCollection services, string connectionString = default)
        {
            IMongoDatabase database = null;

            if (connectionString == default)
            {
                var runner = MongoDbRunner.StartForDebugging();
                var client = new MongoClient(runner.ConnectionString);
                client.DropDatabase("debug");
                database = client.GetDatabase("debug");
            }
            else
            {
                var dbname = MongoUrl.Create(connectionString).DatabaseName;
                var client = new MongoClient(connectionString);
                database = client.GetDatabase(dbname);
            }

            services.AddSingleton(database);
            services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));

            foreach (var type in Assembly.GetCallingAssembly().GetTypes()
                .Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(IEntity))))
            {

                services.AddSingleton(type);
            }

            return services;
        }

    }
}