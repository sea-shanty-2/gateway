using System;
using System.Collections.Generic;
using GraphQL.Validation;
using Microsoft.AspNetCore.Http;

namespace Gateway
{
    public class GraphQLOptions
    {
        public PathString Path { get; set; } = "/";
        public bool ExposeExceptions { get; set; } = false;
        public bool EnableMetrics { get; set; } = true;
        public Func<HttpContext, object> BuildUserContext { get; set; }
        public IEnumerable<IValidationRule> ValidationRules { get; set; }
    }
}