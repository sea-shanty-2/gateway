using Gateway.GraphQL.Mutations;
using GraphQL;
using GraphQL.Types;

namespace Gateway.GraphQL
{
    public class Mutation : ObjectGraphType<object>
    {
        public Mutation()
        {
            Field<BroadcastMutation>("broadcasts", resolve: ctx => new { });
            Field<AccountMutation>("accounts", resolve: ctx => new { });
        }
    }
}