using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Gateway.GraphQL.Extensions;
using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SelectionBroadcast = EnvueStreamSelection.Broadcast;
using EnvueStreamSelection.Broadcast.Rating;
using EnvueStreamSelection;
using EnvueStreamSelection.Selector.EpsilonGreedy;
using EnvueStreamSelection.Selector.Autonomous;
using System.Net;

namespace Gateway.GraphQL.Queries
{
    public class EventQuery : ObjectGraphType<object>
    {
        private const int ExplicitRatingWeight = 5;
        private const int ImplicitRatingWeight = 1;

        public EventQuery(IRepository<Broadcast> broadcastRepository, IRepository<Viewer> viewerRepository, IConfiguration configuration)
        {
            FieldAsync<ListGraphType<EventType>>("all", resolve: async context =>
            {
                // Get events from the clustering service
                var client = new HttpClient
                {
                    BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                };

                var response = await client.GetAsync("clustering/events", context.CancellationToken);

                // Throw error if request was unsuccessful
                if (!response.IsSuccessStatusCode)
                {
                    context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                    return default;
                }

                // Parse the response data
                var data = await response.Content.ReadAsStringAsync();
                var jArray = JArray.Parse(data);

                // Construct the event entities
                var events = new List<Event>();

                try
                {
                    foreach (var e in jArray.Children())
                    {
                        var broadcasts = new List<Broadcast>();

                        foreach (JObject obj in e.Children())
                        {
                            // Lookup each broadcast entity in the database and add to the list of broadcasts for the event
                            var broadcast = await broadcastRepository.FindAsync(x => x.Id == obj.GetValue("id").ToObject<string>());

                            if (broadcast != null)
                            {
                                broadcasts.Add(broadcast);
                            }
                        }

                        // Create the event if the list of broadcasts is not empty
                        if (broadcasts.Any())
                        {
                            events.Add(new Event
                            {
                                Broadcasts = broadcasts
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    context.Errors.Add(new ExecutionError(e.Message));
                    return default;
                }

                return events;
            });

            FieldAsync<EventType>(
                "containing",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "id",
                        Description = "Unique identifier of the broadcast whose event is to be found.",
                    }),

                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");

                    var broadcast = await broadcastRepository.FindAsync(x => x.Id == id, context.CancellationToken);

                    if (broadcast == null)
                    {
                        context.Errors.Add(new ExecutionError($"Broadcast {id} not found"));
                        return default;
                    }

                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                    };

                    if (broadcast.Expired == true)
                    {
                        await client.PostAsJsonAsync("/data/remove", new { id = id });
                    }

                    var response = await client.GetAsync("clustering/events", context.CancellationToken);

                    // Throw error if request was unsuccessful
                    if (!response.IsSuccessStatusCode)
                    {
                        context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                        return default;
                    }

                    // Parse the response data
                    var data = await response.Content.ReadAsStringAsync();
                    var jArray = JArray.Parse(data);

                    IEnumerable<Broadcast> broadcasts = default;

                    try
                    {
                        foreach (var e in jArray.Children())
                        {
                            var ids = e.Children<JObject>().Select(b => b.Value<string>("id"));

                            if (ids.Contains(id))
                            {
                                broadcasts = await broadcastRepository.FindRangeAsync(x => x.Expired != true && ids.Contains(id));
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        context.Errors.Add(new ExecutionError(e.Message));
                        return default;
                    }

                    if (broadcasts == default || !broadcasts.Any())
                    {
                        context.Errors.Add(new ExecutionError($"Event containing broadcast {id} was not found"));
                        return default;
                    }

                    var selectionBroadcasts = broadcasts.Select(
                        x => new SelectionBroadcast.Broadcast()
                        {
                            Stability = (float)x.Stability.GetValueOrDefault(),
                            Bitrate = x.Bitrate.GetValueOrDefault(),
                            Identifier = x.Id,
                            Ratings = new List<IBroadcastRating>
                            {
                                new BroadcastRating(RatingPolarity.Positive, ExplicitRatingWeight * x.PositiveRatings.GetValueOrDefault()),
                                new BroadcastRating(RatingPolarity.Negative, ExplicitRatingWeight * x.NegativeRatings.GetValueOrDefault())
                            }
                        } as SelectionBroadcast.IBroadcast
                    );

                    var epsilon = new EpsilonGreedySelector(
                        new ExponentialEpsilonComputer(0.15, 0.05),
                        new BestBroadcastSelector(),
                        new AutonomousBroadcastSelector()
                    );


                    var recommended = epsilon.SelectFrom(selectionBroadcasts.ToList());

                    return new Event {
                        Broadcasts = broadcasts,
                        Recommended = broadcasts.FirstOrDefault(x => x.Id == recommended.Identifier)
                    };
                });

        }
    }
}