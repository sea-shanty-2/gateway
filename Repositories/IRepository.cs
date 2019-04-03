

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
using MongoDB.Bson;

namespace Gateway.Repositories
{
    public interface IRepository
    {
        Task<T> AddOneAsync<T>(T item, CancellationToken cancellationToken = default) where T : IEntity;
        Task<IEnumerable<T>> AddManyAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default) where T : IEntity;
        Task<T> FindOneAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : IEntity;
        Task<IEnumerable<T>> FindManyAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : IEntity;
        Task<T> UpdateOneAsync<T>(Expression<Func<T, bool>> expression, BsonDocument item, bool upsert = false, CancellationToken cancellationToken = default) where T : IEntity;
        Task<IEnumerable<T>> UpdateManyAsync<T>(Expression<Func<T, bool>> expression, BsonDocument item, bool upsert = false, CancellationToken cancellationToken = default) where T : IEntity;
        Task DeleteAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : IEntity;
        Connection<T> Connection<T, U>(Expression<Func<T, bool>> expression, ResolveConnectionContext<U> context) where T : IEntity;
    }
}