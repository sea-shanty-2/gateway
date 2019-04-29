using System;
using System.Collections.Generic;
using System.Net.Http;
using Gateway.GraphQL.Extensions;
using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Gateway.GraphQL.Queries
{
    public class EventQuery : ObjectGraphType<object>
    {
        public EventQuery(IRepository<Broadcast> repository, IConfiguration configuration)
        {
            FieldAsync<ListGraphType<EventType>>("all", resolve: async context =>
            {
                /* var client = new HttpClient
                {
                    BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                };

                var response = await client.GetAsync("clustering/events");

                return await response.Content.ReadAsStringAsync(); */
                var events = new List<Event> {
                    new Event {
                        Broadcasts = await repository.FindRangeAsync(_ => true)
                    }
                };

                return events;
            });
            /* Connection<BroadcastType>()
                .Name("page")
                .Description("Gets pages of events consisting of pages of broadcasts for the event.")
                .Bidirectional()
                .ResolveAsync(async context =>
                {
                    var entities = await repository.FindRangeAsync(_ => true);
                    return entities.ToConnection(context);
                }); */
        }
    }
}