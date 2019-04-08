using System;
using GraphQL;

namespace Gateway.GraphQL
{
    /// <summary>
    /// Provides dependency resolution for GraphQL using an <seealso cref="IServiceProvider"/>
    /// </summary>
    public sealed class DependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _services;

        public DependencyResolver(IServiceProvider services)
        {
            _services = services;
        }

        public T Resolve<T>() => (T)_services.GetService(typeof(T));

        public object Resolve(Type type) => _services.GetService(type);
    }
}