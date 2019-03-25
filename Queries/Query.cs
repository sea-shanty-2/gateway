using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Gateway.Models;
using GraphQL;
using Gateway.Repositories;
using Gateway.Types;
using Gateway.Extensions;

namespace Gateway.Queries
{
    public class Query : ObjectGraphType<object>
    {
        public Query()
        {
            Field<AccountQuery>("accounts", resolve: ctx => new {});
        }

    
    }
}