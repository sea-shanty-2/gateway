using System;
using Microsoft.AspNetCore.Builder;

namespace Gateway.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGraphQL(this IApplicationBuilder app, Action<GraphQLOptions> config)
        {
            var options =  new GraphQLOptions();
            config.Invoke(options);
            return app.UseMiddleware<GraphQLMiddleware>(options);
        }
    }
}