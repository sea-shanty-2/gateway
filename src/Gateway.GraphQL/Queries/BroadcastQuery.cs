using System;
using Gateway.GraphQL.Extensions;
using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http.Internal;
using MongoDB.Driver;

namespace Gateway.GraphQL.Queries
{
    public class BroadcastQuery : ObjectGraphType<object>
    {
        public BroadcastQuery(IRepository<Broadcast> repository)
        {

            FieldAsync<BroadcastType>(
                "single",
                "Get an broadcast by its unique identifier.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "id",
                        Description = "The unique identifier of the broadcast.",
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return await repository.FindAsync(x => x.Id == id, context.CancellationToken);
                });

            Field<IntGraphType>(
                "viewer_count",
                "Get the number of viewers on a specific broadcast",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "id",
                        Description = "Unique identifier of the broadcast."
                    }), 
                resolve: context =>
                {
                    return 0;
                },
                "View count is present on the broadcast return type");

            Connection<BroadcastType>()
                .Name("page")
                .Description("Gets pages of broadcasts.")
                .Bidirectional()
                .ResolveAsync(async context =>
                {
                    var entities = await repository.FindRangeAsync(_ => true, context.CancellationToken);
                    return entities.ToConnection(context);
                });

            Connection<BroadcastType>()
                .Name("active")
                .Description("Gets pages of active broadcasts.")
                .Bidirectional()
                .ResolveAsync(async context => {
                    var expiration = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2));
                    var entities = await repository.FindRangeAsync(x => x.Activity.CompareTo(expiration) > 0 && x.Expired != true);
                    var a = entities.ToConnection(context);
                    return a;
                });
        }
    }
}