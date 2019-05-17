
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gateway.GraphQL.Services
{
    public class BroadcastService : IHostedService
    {
        private Timer timer;
        private readonly IRepository<Broadcast> repository;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        public BroadcastService(IRepository<Broadcast> repository, IConfiguration configuration, ILogger<BroadcastService> logger)
        {
            this.repository = repository;
            this.configuration = configuration;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(
                TimedCleanAsync,
                null,
                TimeSpan.FromMinutes(0),
                TimeSpan.FromMinutes(1)
            );

            logger.LogInformation("Starting broadcast cleaning service");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping broadcast cleaning service");
            timer.Dispose();
            return Task.CompletedTask;
        }

        private async void TimedCleanAsync(object state)
        {
            var inactiveLimit = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            Expression<Func<Broadcast, bool>> filter = x => x.Expired == false && x.Activity.CompareTo(inactiveLimit) <= 0;

            var broadcasts = await repository.FindRangeAsync(filter);
            var first = broadcasts.FirstOrDefault();

            if (first == null)
            {
                return;
            }

            logger.LogDebug("Inactive broadcast: {object}", new { id = first.Id, timestamp = first.Activity.Ticks });

            var client = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
            };

            var response = await client.PostAsJsonAsync("/data/remove-range", broadcasts.Select(x => x.Id));

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "{uri} ({status}): delete {id}",
                    client.BaseAddress.ToString(),
                    response.StatusCode.ToString(),
                    first.Id);
                return;
            }

            client = new HttpClient
            {
                BaseAddress = new Uri($"{configuration.GetValue<string>("RELAY_URL")}")
            };

            HttpStatusCode statusCode = default;

            foreach (var id in broadcasts.Select(x => x.Id))
            {
                response = await client.DeleteAsync(id);
                if (response.IsSuccessStatusCode)
                {
                    await repository.UpdateAsync(
                        x => x.Id == id,
                        new Broadcast { Expired = true, Activity = DateTime.UtcNow });
                }
                else
                {
                    if (statusCode == response.StatusCode)
                        continue;
                    
                    logger.LogError(
                            "{uri} ({status}): delete {id}",
                            client.BaseAddress.ToString(),
                            response.StatusCode.ToString(),
                            id);
                    statusCode = response.StatusCode;
                }
            }
        }
    }
}