using GraphQL;
using GraphQL.Types;

namespace Gateway.Mutations
{
    public class Mutation : ObjectGraphType<object>
    {
        public Mutation()
        {
            Field<AccountMutation>("accounts", resolve: ctx => new {});
            Field<BroadcastMutation>("broadcasts", resolve: ctx => new {});
        }
    }
}