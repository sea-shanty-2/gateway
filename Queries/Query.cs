using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Gateway.Models;
using GraphQL;
using Gateway.Repositories;
using Gateway.Types;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Gateway.Queries
{
    public class Query : ObjectGraphType<object>
    {
        public Query(IConfiguration configuration)
        {
            Field<AccountQuery>("accounts", resolve: ctx => new { });
            Field<BroadcastQuery>("broadcasts", resolve: ctx => new { });
            Field<AuthenticationQuery>("authenticate", resolve: ctx => new { });
        }


    }
}