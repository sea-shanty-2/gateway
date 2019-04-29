using System;
using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using GraphQL.Authorization;
using System.Net.Http;
using GraphQL;
using System.Collections.Generic;

namespace Gateway.GraphQL.Mutations
{
    public class BroadcastMutation : ObjectGraphType<object>
    {
        public BroadcastMutation(IRepository<Broadcast> repository, IConfiguration configuration)
        {

            this.FieldAsync<BroadcastCreateType>(
                "create",
                "Create a broadcast and obtain the rtmp url",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<BroadcastInputType>>()
                    {
                        Name = "broadcast"
                    }),
                resolve: async context =>
                {
                    var broadcast = context.GetArgument<Broadcast>("broadcast");
                    broadcast = await repository.AddAsync(broadcast, context.CancellationToken);

                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                    };

                    var response = await client.PostAsJsonAsync(
                        "/data/add",
                        new List<Broadcast> { broadcast },
                        context.CancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        /// TODO: Deserialize JSON and instantiate execution error properly.
                        /// https://graphql.org/learn/validation/ 
                        var error = await response.Content.ReadAsStringAsync();
                        context.Errors.Add(new ExecutionError("error"));
                        return default;
                    }

                    return broadcast;
                });


            this.FieldAsync<BroadcastType>(
                "update",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<BroadcastUpdateInputType>>()
                    {
                        Name = "broadcast"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    var broadcast = context.GetArgument<Broadcast>("broadcast");
                    broadcast.Activity = DateTime.UtcNow;
                    broadcast = await repository.UpdateAsync(x => x.Id == id, broadcast, context.CancellationToken);

                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                    };

                    var response = await client.PostAsJsonAsync("/data/update", broadcast, context.CancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        /// TODO: Deserialize JSON and instantiate execution error properly.
                        /// https://graphql.org/learn/validation/ 
                        var error = await response.Content.ReadAsStringAsync();
                        context.Errors.Add(new ExecutionError("error"));
                        return default;
                    }

                    return broadcast;
                });

            this.FieldAsync<IdGraphType>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    await repository.RemoveAsync(x => x.Id == id, context.CancellationToken);
                    return id;
                });
        }
    }
}