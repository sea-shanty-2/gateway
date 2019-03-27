using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Data;
using Gateway.Models;
using GraphQL.Builders;
using GraphQL.Types.Relay.DataObjects;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway.Repositories
{
    public class Repository : IRepository
    {
        private readonly IDatabase _database;

        public Repository(IDatabase database)
        {
            _database = database;
        }

        public async Task<T> AddAsync<T>(T item, CancellationToken cancellationToken = default) where T : Entity
        {
            await _database.GetCollection<T>().InsertOneAsync(item, new InsertOneOptions(), cancellationToken);
            var items = await _database.GetCollection<T>().FindAsync(Builders<T>.Filter.Eq(x => x.Id, item.Id), new FindOptions<T, T>(), cancellationToken);
            return await items.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> AddAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default) where T : Entity
        {
            await _database.GetCollection<T>().InsertManyAsync(items, new InsertManyOptions(), cancellationToken);

            var cursor = await _database.GetCollection<T>()
                .FindAsync(
                    Builders<T>.Filter.Where(x => items.Select(y => y.Id).Contains(x.Id)),
                    new FindOptions<T, T>(),
                    cancellationToken
                );

            return cursor.ToEnumerable(cancellationToken);
        }


        public async Task DeleteAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : Entity
        {
            await _database.GetCollection<T>().DeleteManyAsync(expression, cancellationToken);
        }

        public async Task<IEnumerable<T>> ManyAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : Entity
        {
            var cursor = await _database.GetCollection<T>().FindAsync(expression, new FindOptions<T, T>(), cancellationToken);
            return cursor.ToEnumerable(cancellationToken);
        }

        public async Task<T> SingleAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : Entity
        {
            var cursor = await _database.GetCollection<T>().FindAsync(expression, new FindOptions<T, T>(), cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> UpdateAsync<T>(Expression<Func<T, bool>> expression, T item, CancellationToken cancellationToken = default) where T : Entity
        {
            var update = new BsonDocument { { "$set", item.ToBsonDocument() } };
            await _database.GetCollection<T>().UpdateManyAsync(expression, update, new UpdateOptions(), cancellationToken);
            return await ManyAsync(expression, cancellationToken);
        }

        public Connection<T> Connection<T, U>(Expression<Func<T, bool>> expression, ResolveConnectionContext<U> context) where T : Entity
        {
            var data = _database.GetCollection<T>().Find(expression, new FindOptions()).ToEnumerable();
            var cancellationToken = context.CancellationToken;
            IEnumerable<T> items;
            var totalCount = data.Count();
            var pageSize = context.PageSize ?? totalCount; 
            
            if (context.IsUnidirectional || context.After != null || context.Before == null)
            {
                if (context.After != null)
                {
                    items = data
                        .SkipWhile(x => x.Id != context.After)
                        .Skip(1)
                        .Take(context.First ?? pageSize).ToList();
                }
                else
                {
                    items = data
                        .Take(context.First ?? pageSize).ToList();
                }
            }
            else
            {
                if (context.Before != null)
                {
                    items = data
                        .Reverse()
                        .SkipWhile(x => x.Id != context.Before)
                        .Skip(1)
                        .Take(context.Last ?? pageSize).ToList();
                }
                else
                {
                    items = data
                        .Reverse()
                        .Take(context.Last ?? pageSize).ToList();
                }
            }

            items = items.ToList();
            var endCursor = totalCount > 0 ? items.Last().Id : null;

            return new Connection<T>
            {
                Edges = items.Select(x => new Edge<T>() { Cursor = x.Id, Node = x }).ToList(),
                TotalCount = totalCount,
                PageInfo = new PageInfo()
                {
                    StartCursor = items.FirstOrDefault()?.Id,
                    EndCursor = items.LastOrDefault()?.Id,
                    HasPreviousPage = items.FirstOrDefault()?.Id != data.FirstOrDefault()?.Id,
                    HasNextPage = items.LastOrDefault()?.Id != data.LastOrDefault()?.Id
                }
            };
        }
    }
}