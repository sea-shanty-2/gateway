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
using System.Diagnostics;
using Bogus.DataSets;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System.Linq;
using Serilog;

namespace Gateway.GraphQL.Mutations
{
    public class BroadcastMutation : ObjectGraphType<object>
    {
        public BroadcastMutation(
            IRepository<Broadcast> broadcasts,
            IRepository<Viewer> viewers,
            IRepository<Account> accounts,
            IConfiguration configuration)
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
                    // Retrieve the broadcast entity from the argument dictionary
                    var broadcast = context.GetArgument<Broadcast>("broadcast");

                    // Add the broadcast entity to the database
                    broadcast.AccountId = context.UserContext.As<UserContext>().User.Identity.Name;
                    broadcast = await broadcasts.AddAsync(broadcast, context.CancellationToken);


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

                    SendNewBroadcastNotification(broadcast.Categories);

                    return broadcast;

                }
            ).AuthorizeWith("AuthenticatedPolicy");


            this.FieldAsync<IdGraphType>(
                "join",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),

                resolve: async context =>
                {
                    // Update the broadcast entity in the database
                    var broadcastId = context.GetArgument<string>("id");
                    var accountId = context.UserContext.As<UserContext>().User.Identity.Name;

                    // Validate the account and broadcast
                    var broadcast = await broadcasts.FindAsync(x => x.Id == broadcastId, context.CancellationToken);

                    if (broadcast == null)
                    {
                        context.Errors.Add(new ExecutionError("Broadcast not found"));
                    }

                    var account = await accounts.FindAsync(x => x.Id == accountId, context.CancellationToken);

                    if (account == null)
                    {
                        context.Errors.Add(new ExecutionError("Account not found"));
                    }

                    // Construct a viewer entity and add it to the repository
                    var viewer = new Viewer
                    {
                        AccountId = accountId,
                        BroadcastId = broadcastId,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };

                    await viewers.AddAsync(viewer, context.CancellationToken);

                    return broadcastId;
                }
            ).AuthorizeWith("AuthenticatedPolicy");

            this.FieldAsync<BroadcastType>(
                "addViewers",
                "(DEBUG) Add viewers to the specified broadcast",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<IntGraphType>>
                    {
                        Name = "quantity"
                    }
                ),
                resolve: async context =>
                {
                    var broadcastId = context.GetArgument<string>("id");
                    var quantity = context.GetArgument<int>("quantity");

                    var broadcast = await broadcasts.FindAsync(x => x.Id == broadcastId, context.CancellationToken);

                    if (broadcast == null)
                    {
                        context.Errors.Add(new ExecutionError("Broadcast not found"));
                    }

                    for (int i = 0; i < quantity; i++)
                    {
                        var viewer = new Viewer
                        {
                            AccountId = Guid.NewGuid().ToString("N"),
                            BroadcastId = broadcastId,
                            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                        };

                        await viewers.AddAsync(viewer, context.CancellationToken);
                    }

                    return broadcast;
                }
            );

            this.FieldAsync<IdGraphType>(
                "leave",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),

                resolve: async context =>
                {
                    // Update the broadcast entity in the database
                    var broadcastId = context.GetArgument<string>("id");
                    var accountId = context.UserContext.As<UserContext>().User.Identity.Name;

                    // Validate the account and broadcast
                    var broadcast = await broadcasts.FindAsync(x => x.Id == broadcastId, context.CancellationToken);

                    if (broadcast == null)
                    {
                        context.Errors.Add(new ExecutionError("Broadcast not found"));
                    }

                    var account = await accounts.FindAsync(x => x.Id == accountId, context.CancellationToken);

                    if (account == null)
                    {
                        context.Errors.Add(new ExecutionError("Account not found"));
                    }

                    // Construct a viewer entity and add it to the repository
                    var viewer = new Viewer
                    {
                        AccountId = accountId,
                        BroadcastId = broadcastId,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };

                    await viewers.AddAsync(viewer, context.CancellationToken);

                    return broadcastId;
                }
            ).AuthorizeWith("AuthenticatedPolicy");

            this.FieldAsync<BroadcastType>(
                "report",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<StringGraphType>>()
                    {
                        Name = "message"
                    }
                ),
                resolve: async context =>
                {
                    // Get the id argument value
                    var id = context.GetArgument<string>("id");

                    // Validate broadcast id
                    var broadcast = await broadcasts.FindAsync(x => x.Id == id, context.CancellationToken);

                    // Inform the user if the id is invalid
                    if (broadcast == default)
                    {
                        context.Errors.Add(new ExecutionError("Invalid broadcast id"));
                        return default;
                    }

                    // Get the message argument value
                    var message = context.GetArgument<string>("message");

                    // Add the report to the broadcast
                    broadcast.Reports.Add(message);

                    // Update the broadcast and return the updated broadcast
                    return await broadcasts.UpdateAsync(x => x.Id == id, broadcast, context.CancellationToken);
                }
            );

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
                    broadcast = await broadcasts.UpdateAsync(x => x.Id == id, broadcast, context.CancellationToken);

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
                }
            );

            this.FieldAsync<BroadcastStopType>(
                "stop",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    var identity = context.UserContext.As<UserContext>().User.Identity;

                    // Get broadcast 
                    var broadcast = await broadcasts.FindAsync(x => x.Id == id, context.CancellationToken);

                    // Set expired to true
                    broadcast.Expired = true;
                    broadcast = await broadcasts.UpdateAsync(x => x.Id == id, broadcast, context.CancellationToken);

                    // Ensure only broadcaster can stop broadcast
                    if (broadcast.AccountId != identity.Name)
                    {
                        context.Errors.Add(new ExecutionError("Not authorized to stop broadcast"));
                        return default;
                    }

                    var dto = new
                    {
                        Id = broadcast.Id,
                        Longitude = broadcast.Location.Longitude,
                        Latitude = broadcast.Location.Latitude,
                        StreamDescription = broadcast.Categories
                    };

                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                    };

                    var response = await client.PostAsJsonAsync(
                        "/data/remove",
                        dto,
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

                    response = await client.DeleteAsync(id);

                    // React accordingly
                    if (!response.IsSuccessStatusCode)
                    {
                        context.Errors.Add(new ExecutionError(response.ReasonPhrase));
                        return default;
                    }

                    return broadcast;
                }
            ).AuthorizeWith("AuthenticatedPolicy");
        }

        private async void SendNewBroadcastNotification(double[] categories)
        {

            // Define a condition which will send to devices which are subscribed
            var condition = CreateCondition(categories);
            if (condition.IsEmpty())
            {
                Log.Error("Invalid condition. Condition cannot be empty.");
            }


            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = "New stream",
                    Body = $"A new stream you might be interested in has begun.",
                    //$"DEBUG: Condition: {condition}, Categories: [{String.Join("", categories)}]",
                },
                Condition = condition,
                Android = new AndroidConfig()
                {
                    // Set the limit where the notification is no longer relevant.
                    TimeToLive = TimeSpan.FromMinutes(15),
                },
            };

            // Send a message to devices subscribed to the combination of topics
            // specified by the provided condition.
            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Firebase Error. Error while sending notification.");
            }
            // Response is a message ID string.


            //Console.WriteLine("Successfully sent message: " + response);

        }
        private string CreateCondition(double[] topics)
        {
            var first = true;
            var condition = new string("");
            for (var i = 0; i < topics.Length; i++)
            {
                switch (topics[i])
                {
                    case 0.0:
                        continue;
                    case 1.0:
                        if (!first)
                            condition += " || ";
                        else
                            first = false;

                        condition += $"'Category{i}' in topics";
                        break;
                    default:
                        Log.Error("Invalid category value");
                        continue;

                }
            }
            return condition;
        }
    }
}