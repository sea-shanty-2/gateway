using Gateway.GraphQL.Queries;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Gateway.GraphQL
{
    public class Query : ObjectGraphType<object>
    {
        public Query(IConfiguration configuration)
        {
            Field<AccountQuery>("accounts", resolve: ctx => new { });
            Field<BroadcastQuery>("broadcasts", resolve: ctx => new { });
            Field<AuthenticationQuery>("authenticate", resolve: ctx => new { });
            Field<EventQuery>("events", resolve: ctx => new {});
        }
    }
}