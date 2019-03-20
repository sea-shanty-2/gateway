using Gateway.Queries;
using GraphQL.Types;

namespace Gateway
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            Field<AccountQuery>("accountQuery", resolve: context => new {});
            Field<BroadcastQuery>("broadcastQuery", resolve: context => new {});
        }
    }
}