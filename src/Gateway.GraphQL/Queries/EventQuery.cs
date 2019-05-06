using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Gateway.GraphQL.Extensions;
using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Gateway.GraphQL.Queries
{
    public class EventQuery : ObjectGraphType<object>
    {
        public EventQuery(IRepository<Broadcast> repository, IConfiguration configuration)
        {
            FieldAsync<ListGraphType<EventType>>("all", resolve: async context =>
            {
                // Get events from the clustering service
                var client = new HttpClient
                {
                    BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                };

                var response = await client.GetAsync("clustering/events");

                // Throw error if request was unsuccessful
                if (!response.IsSuccessStatusCode)
                {
                    context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                    return default;
                }

                // Parse the response data
                var data = await response.Content.ReadAsStringAsync();
                var jarray = JArray.Parse(data);

                // Construct the event entities
                var events = new List<Event>();

                try
                {
                    foreach (var e in jarray.Children())
                    {
                        var broadcasts = new List<Broadcast>();

                        foreach (JObject b in e.Children())
                        {
                            // Lookup each broadcast entity in the database and add to the list of broadcasts for the event
                            var broadcast = await repository.FindAsync(x => x.Id == b.GetValue("id").ToObject<string>());

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
                    var queriedId = context.GetArgument<string>("id");
                    
                    // Get events from the clustering service
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                    };
    
                    var response = await client.GetAsync("clustering/events");
    
                    // Throw error if request was unsuccessful
                    if (!response.IsSuccessStatusCode)
                    {
                        context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                        return default;
                    }
    
                    // Parse the response data
                    var data = await response.Content.ReadAsStringAsync();
                    var jarray = JArray.Parse(data);
                    
                    // Prepare return event
                    var queriedEvent = new Event();
    
                    try
                    {
                        // Go through all event clusters
                        foreach (var e in jarray.Children())
                        {
                            var broadcastIds = e.Children<JObject>().Select(b => b.GetValue("id").ToObject<string>()).ToArray();
                            // If an event contains our broadcaster, return that event
                            if (broadcastIds.Any(id => id == queriedId))
                            {
                                queriedEvent.Broadcasts = broadcastIds.Select(id =>
                                    repository.FindAsync(b => b.Id == id).GetAwaiter().GetResult());
                            }
                        }
                    }
                    
                    catch (Exception e)
                    {
                        context.Errors.Add(new ExecutionError(e.Message));
                        return default;
                    }

                    return queriedEvent;
                });

        }
    }
}