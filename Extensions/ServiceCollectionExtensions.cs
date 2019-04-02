using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GraphQL.Types.Relay;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Extensions
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
    }
}