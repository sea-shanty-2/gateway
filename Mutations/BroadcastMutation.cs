using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway.Mutations
{
    public class BroadcastMutation : ObjectGraphType<object>
    {
        public BroadcastMutation(IRepository repository)
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
                    return await repository.AddOneAsync(broadcast, context.CancellationToken);
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
                    
                    return await repository.UpdateOneAsync<Broadcast>(
                        x => x.Id == id,
                        new BsonDocument { { "$set", broadcast.ToBsonDocument() } },
                        cancellationToken: context.CancellationToken
                    );
                });

            this.FieldAsync<BroadcastDeleteType>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    await repository.DeleteAsync<Broadcast>(x => x.Id == id, context.CancellationToken);
                    return id;
                });
        }
    }
}