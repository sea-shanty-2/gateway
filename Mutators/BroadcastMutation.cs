using Gateway.Models;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Mutators
{
    public class BroadcastMutation : ObjectGraphType
    {
        public BroadcastMutation(Data data)
        {
            Field<BooleanGraphType>(
                "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<BroadcastInputGraphType>> { Name = "broadcast" }
                ),
                resolve: context =>
                {
                    var broadcast = context.GetArgument<Broadcast>("broadcast");

                    data.GetCollection<Broadcast>().InsertOne(broadcast);
                    return true;
                }
            );

            FieldAsync<BooleanGraphType>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "The ID of the broadcast." }
                ),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return (await data.GetCollection<Broadcast>().DeleteOneAsync(x => x.Id == id)).IsAcknowledged;
                }
            );
        }

    }
}