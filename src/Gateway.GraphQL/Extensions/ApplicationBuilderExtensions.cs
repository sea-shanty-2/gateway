using System;
using Microsoft.AspNetCore.Builder;

namespace Gateway.GraphQL.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGraphQL(this IApplicationBuilder app, Action<Options> config)
        {
            var options = new Options();
            config.Invoke(options);
            return app.UseMiddleware<Middleware>(options);
        }

        public static IApplicationBuilder RunSeeder(this IApplicationBuilder app)
        {

            return app;
        }
    }
}