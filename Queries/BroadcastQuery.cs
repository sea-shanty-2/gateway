using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Queries
{
    public class BroadcastQuery : ObjectGraphType<object>
    {
        public BroadcastQuery(IRepository repository)
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
                    return await repository.FindOneAsync<Broadcast>(x => x.Id == id, context.CancellationToken);
                });

            Connection<BroadcastType>()
                .Name("page")
                .Description("Gets pages of broadcasts.")
                .Bidirectional()
                // Set the maximum size of a page, use .ReturnAll() to set no maximum size.
                .Resolve(context => {
                    return repository.Connection<Broadcast, object>(_ => true, context);
                });

            Connection<BroadcastType>()
                .Name("active")
                .Description("Gets active broadcasts.")
                .Bidirectional()
                // Set the maximum size of a page, use .ReturnAll() to set no maximum size.
                .Resolve(context => {
                    return repository.Connection<Broadcast, object>(x => x.Ended == default, context);
                });
        }
    }
}