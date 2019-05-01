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
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace Gateway.GraphQL.Mutations
{
    public class BroadcastMutation : ObjectGraphType<object>
    {
        public BroadcastMutation(IRepository<Broadcast> repository, IConfiguration configuration)
        {

            this.FieldAsync<BroadcastCreateType>(
                "create",
                "Create a broadcast and obtain the RTMP url",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<BroadcastInputType>>()
                    {
                        Name = "broadcast"
                    }),
                resolve: async context =>
                {
                    // Add the broadcast entity to the database
                    var broadcast = context.GetArgument<Broadcast>("broadcast");
                    broadcast.AccountId = context.UserContext.As<UserContext>().User.Identity.Name;
                    broadcast = await repository.AddAsync(broadcast, context.CancellationToken);


                    // Construct a data transfer object for the clustering service
                    var dto = new
                    {
                        Id = broadcast.Id,
                        Longitude = broadcast.Location.Longitude,
                        Latitude = broadcast.Location.Latitude,
                        StreamDescription = broadcast.Categories
                    };

                    // Send the DTO to the clustering service
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                    };

                    var response = await client.PostAsJsonAsync(
                        "/data/add",
                        new object[] { dto },
                        context.CancellationToken);

                    // React accordingly
                    if (!response.IsSuccessStatusCode)
                    {
                        context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                        return default;
                    }

                    
                    client = new HttpClient
                    {
                        BaseAddress = new Uri($"{configuration.GetValue<string>("RELAY_URL")}")
                    };

                    // Create key value pairs.
                    var keyValues = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>(
                            "stream_url",
                            $"{configuration.GetValue<string>("LIVESTREAM_URL")}/{broadcast.Token}.m3u8"
                        )
                    };

                    var content = new FormUrlEncodedContent(keyValues);
                    response = await client.PostAsync(broadcast.Id, content);

                    // React accordingly
                    if (!response.IsSuccessStatusCode)
                    {
                        context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                        return default;
                    }

                    return broadcast;

                }).AuthorizeWith("AuthenticatedPolicy");


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
                    // Update the broadcast entity in the database
                    var id = context.GetArgument<string>("id");
                    var broadcast = context.GetArgument<Broadcast>("broadcast");

                    // Check if the location was changed
                    var locationUpdated = broadcast.Location != null;
                    broadcast.Activity = DateTime.UtcNow;
                    broadcast = await repository.UpdateAsync(x => x.Id == id, broadcast, context.CancellationToken);

                    if (locationUpdated)
                    {
                        // Construct a data transfer object for the clustering service
                        var dto = new
                        {
                            Id = broadcast.Id,
                            Longitude = broadcast.Location.Longitude,
                            Latitude = broadcast.Location.Latitude,
                            StreamDescription = broadcast.Categories
                        };

                        // Send the DTO to the clustering service
                        var client = new HttpClient
                        {
                            BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                        };

                        var response = await client.PostAsJsonAsync("/data/update", broadcast, context.CancellationToken);

                        // React accordingly
                        if (!response.IsSuccessStatusCode)
                        {
                            context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                            return default;
                        }
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