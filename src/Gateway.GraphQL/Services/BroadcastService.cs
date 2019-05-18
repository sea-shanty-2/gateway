
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
            logger.LogInformation("Starting broadcast cleaning task");

            var inactiveLimit = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            Expression<Func<Broadcast, bool>> filter = x => x.Expired != true && x.Activity.Value.CompareTo(inactiveLimit) <= 0;

            var broadcasts = await repository.FindRangeAsync(filter);
            var first = broadcasts.FirstOrDefault();

            if (first == null)
            {
                return;
            }

            logger.LogDebug("Inactive broadcast: {object}", new { id = first.Id, timestamp = first.Activity.Value.Ticks });

            

            HttpResponseMessage response = default;

            var clusteringClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
            };

            var relayClient = new HttpClient
            {
                BaseAddress = new Uri($"{configuration.GetValue<string>("RELAY_URL")}")
            };

            HttpStatusCode statusCode = default;

            foreach (var id in broadcasts.Select(x => x.Id))
            {
                try
                {
                    await relayClient.DeleteAsync(id);
                    response = await clusteringClient.PostAsJsonAsync("/data/remove", new { id = id });
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
                {
                    await repository.UpdateAsync(
                        x => x.Id == id,
                        new Broadcast { Expired = true, Activity = DateTime.UtcNow });
                    
                    logger.LogDebug(
                        "Broadcast {id} stopped due to inactivity",
                        id);
                }
                else
                {
                    if (statusCode == response.StatusCode)
                        continue;

                    logger.LogError(
                        "{status}: {message}",
                        response.StatusCode.ToString(),
                        await response.Content.ReadAsStringAsync());

                    statusCode = response.StatusCode;
                }
            }
        }
    }
}