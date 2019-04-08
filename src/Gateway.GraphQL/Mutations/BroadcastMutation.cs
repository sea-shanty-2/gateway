using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway.GraphQL.Mutations
{
    public class BroadcastMutation : ObjectGraphType<object>
    {
        public BroadcastMutation(IRepository<Broadcast> repository)
        {

            this.FieldAsync<BroadcastCreateType>(
                "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<BroadcastInputType>>()
                    {
                        Name = "broadcast"
                    }),
                resolve: async context =>
                {
                    var broadcast = context.GetArgument<Broadcast>("broadcast");
                    return await repository.AddAsync(broadcast, context.CancellationToken);
                });



            this.FieldAsync<BroadcastUpdateType>(
                "update",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<BroadcastInputType>>()
                    {
                        Name = "broadcast"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    var broadcast = context.GetArgument<Broadcast>("broadcast");
                    
                    return await repository.UpdateAsync(x => x.Id == id, broadcast, context.CancellationToken);
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