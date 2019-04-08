using System;
using System.Linq;
using System.Reflection;
using Gateway.GraphQL.Requirements;
using GraphQL.Authorization;
using GraphQL.Types;
using GraphQL.Types.Relay;
using GraphQL.Validation;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.GraphQL.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddRelayGraphTypes(this IServiceCollection services) => services
            .AddSingleton(typeof(ConnectionType<>))
            .AddSingleton(typeof(EdgeType<>))
            .AddSingleton<PageInfoType>();

        public static IServiceCollection AddGraphTypes(this IServiceCollection services)
        {
            foreach (var type in Assembly.GetCallingAssembly().GetTypes()
                .Where(x => !x.IsAbstract && typeof(IGraphType).IsAssignableFrom(x)))
            {
                services.AddSingleton(type);
            }
            return services;
        }

        public static IServiceCollection AddGraphQLAuth(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();

            services.AddSingleton(s =>
            {
                var authSettings = new AuthorizationSettings();
                authSettings.AddPolicy("AuthenticatedPolicy", p => p.AddRequirement(new AuthenticatedUserRequirement()));
                return authSettings;
            });

            return services;
        }

    }
}