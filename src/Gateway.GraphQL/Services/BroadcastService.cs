
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.Repositories;
using Microsoft.Extensions.Configuration;

namespace Gateway.GraphQL.Services
{
    public class BroadcastService
    {
        private IRepository<Broadcast> repository;
        private IConfiguration configuration;
        public BroadcastService(IRepository<Broadcast> repository, IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;
        }

        
        /// <summary>
        /// Expires all broadcasts and sends a request to the clustering API to clear data
        /// </summary>
        public async Task Clear()
        {
            await repository.UpdateRangeAsync(
                x => x.Expired == false, 
                new Broadcast
                {
                    Expired = true,
                    Activity = DateTime.UtcNow
                }
            );

            var client = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
            };

            await client.GetAsync("/data/clear");
        }

    }
}