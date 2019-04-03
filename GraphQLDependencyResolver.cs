using System;
using GraphQL;

namespace Gateway
{
    /// <summary>
    /// Provides dependency resolution for GraphQL using an <seealso cref="IServiceProvider"/>
    /// </summary>
    public sealed class GraphQLDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _services;

        public GraphQLDependencyResolver(IServiceProvider services)
        {
            _services = services;
        }

        public T Resolve<T>() => (T)_services.GetService(typeof(T));

        public object Resolve(Type type) => _services.GetService(type);
    }
}