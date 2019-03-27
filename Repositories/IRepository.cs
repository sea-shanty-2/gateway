

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace Gateway.Repositories
{
    public interface IRepository
    {
        Task<T> AddAsync<T>(T item, CancellationToken cancellationToken = default) where T : Entity;
        Task<IEnumerable<T>> AddAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default) where T : Entity;
        Task<T> SingleAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : Entity;
        Task<IEnumerable<T>> ManyAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : Entity;
        Task<IEnumerable<T>> UpdateAsync<T>(Expression<Func<T, bool>> expression, T item, CancellationToken cancellationToken = default) where T : Entity;
        Task DeleteAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : Entity;
        Connection<T> Connection<T, U>(Expression<Func<T, bool>> expression, ResolveConnectionContext<U> context) where T : Entity;
    }
}