
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

            var ids = (await repository.FindRangeAsync(filter)).Select(x => x.Id);

            var client = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
            };

            var response = await client.PostAsJsonAsync("/data/remove-range", ids);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            client = new HttpClient
            {
                BaseAddress = new Uri($"{configuration.GetValue<string>("RELAY_URL")}")
            };

            foreach (var id in ids)
            {
                response = await client.DeleteAsync(id);
                if (response.IsSuccessStatusCode)
                {
                    await repository.UpdateAsync(
                        x => x.Id == id, 
                        new Broadcast { Expired = true, Activity = DateTime.UtcNow });
                }
            }




        }
    }
}