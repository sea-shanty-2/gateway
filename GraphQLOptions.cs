using System;
using System.Collections.Generic;
using GraphQL.Validation;
using Microsoft.AspNetCore.Http;

namespace Gateway
{
    public class GraphQLOptions
    {
        public PathString Path { get; set; } = "/graphql";
        public bool ExposeExceptions { get; set; } = false;
        public bool EnableMetrics { get; set; } = true;
    }
}