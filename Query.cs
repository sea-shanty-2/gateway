using Gateway.Queries;
using GraphQL.Types;

namespace Gateway
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            Field<AccountQuery>("accountQuery", resolve: context => new {});
            Field<StreamQuery>("streamQuery", resolve: context => new {});
        }
    }
}