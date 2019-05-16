
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Gateway.GraphQL.Services
{
    public class BroadcastService : IHostedService
    {
        private IRepository<Broadcast> repository;
        private IConfiguration configuration;
        private Timer timer;
        public BroadcastService(IRepository<Broadcast> repository, IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(
                TimedCleanAsync,
                null,
                TimeSpan.FromMinutes(0),
                TimeSpan.FromMinutes(1)
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            return Task.CompletedTask;
        }

        private async void TimedCleanAsync(object state)
        {
            var inactiveLimit = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            Expression<Func<Broadcast, bool>> filter = x => x.Expired == false && x.Activity.CompareTo(inactiveLimit) <= 0;

            var broadcasts = (await repository.FindRangeAsync(filter)).ToList();

            var client = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
            };

            var response = await client.PostAsJsonAsync("/data/remove-range", broadcasts.Select(x => x.Id));

            if (response.IsSuccessStatusCode)
            {
                await repository.UpdateRangeAsync(filter, new Broadcast { Expired = true, Activity = DateTime.UtcNow });
            }
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