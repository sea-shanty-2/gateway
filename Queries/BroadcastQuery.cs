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

            FieldAsync<BroadcastGraphType, Broadcast>(
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
                    return await repository.SingleAsync<Broadcast>(x => x.Id == id, context.CancellationToken);
                });

            Connection<BroadcastGraphType>()
                .Name("page")
                .Description("Gets pages of broadcasts.")
                .Bidirectional()
                // Set the maximum size of a page, use .ReturnAll() to set no maximum size.
                .Resolve(context => {
                    return repository.Connection<Broadcast, object>(_ => true, context);
                });
        }
    }
}