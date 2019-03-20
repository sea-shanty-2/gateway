using Gateway.Models;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Queries
{
    public class BroadcastQuery : ObjectGraphType
    {
        public BroadcastQuery(Data data)
        {
            FieldAsync<ListGraphType<BroadcastGraphType>>(
                "broadcasts",
                resolve: async context => {
                    return await data.GetCollection<Broadcast>().Find(_ => true).ToListAsync();
                }
            );

            FieldAsync<BroadcastGraphType>(
                "broadcast",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "The ID of the broadcast." }),
                resolve: async context => {
                    var id = context.GetArgument<string>("id");
                    return await data.GetCollection<Broadcast>().Find(x => x.Id == id).FirstOrDefaultAsync();
                }
            );
        }
    }
}