using System;
using System.Linq;
using System.Reflection;
using Gateway;
using Gateway.Models;
using Gateway.MongoDB.Repositories;
using Gateway.Repositories;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="connectionString"></param>
        /// <param name="drop">If true, the existing data (if any) will be dropped.</param>
        /// <returns></returns>
        public static IServiceCollection AddMongoDBRepositories(
            this IServiceCollection services, 
            string connectionString, 
            bool drop = false)
        {
            var dbname = MongoUrl.Create(connectionString).DatabaseName;
            var client = new MongoClient(connectionString);

            if (drop) {
                client.DropDatabase(dbname);
            }

            var database = client.GetDatabase(dbname);

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