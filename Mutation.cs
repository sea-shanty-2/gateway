using Gateway.Models;
using Gateway.Mutators;
using Gateway.Types;
using GraphQL.Types;

namespace Gateway
{
    public class Mutation : ObjectGraphType
    {
        public Mutation()
        {
            Field<AccountMutation>("accountMutation", resolve: context => new {});
            Field<BroadcastMutation>("broadcastMutation", resolve: context => new {});
        }
    }
}