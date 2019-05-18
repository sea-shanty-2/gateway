using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Gateway.MongoDB.Repositories
{
    public class Repository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> _collection;
        public Repository(IMongoDatabase database)
        {
            /// Ignore default values
            /// http://mongodb.github.io/mongo-csharp-driver/2.4/reference/bson/mapping/conventions/
            ConventionRegistry.Register("IgnoreIfNull",
                            new ConventionPack { 
                                new IgnoreIfNullConvention(true) 
                            },
                            t => true);

            /// Map string id to objectid (required for unique indexing in mongodb).
            /// It is very important that the registration of class maps occur prior to them being needed.
            /// http://mongodb.github.io/mongo-csharp-driver/2.2/reference/bson/mapping/
            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(x => x.Id)
                    .SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId));
            });

            _collection = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(
                entity,
                new InsertOneOptions
                {

                },
                cancellationToken
            );

            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _collection.InsertManyAsync(
                entities,
                new InsertManyOptions
                {

                },
                cancellationToken
            );
            return entities;
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            T entity = default;

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var cursor = await _collection.FindAsync(
                    expression,
                    new FindOptions<T, T>
                    {
                        Limit = 1
                    },
                    cancellationToken
                );
                entity = await cursor.FirstOrDefaultAsync(cancellationToken);

                if (entity != null)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            return entity;
        }

        public async Task<IEnumerable<T>> FindRangeAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken)
        {
            IEnumerable<T> entities = default;

            for (var attempt = 0; attempt < 3; attempt++)
            {
                entities = await Task.Run(() => _collection.AsQueryable().Where(expression).AsEnumerable(), cancellationToken);

                if (entities.Any())
                {
                    break;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            return entities;
        }


        public async Task RemoveAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            DeleteResult deletion = default;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                deletion = await _collection.DeleteOneAsync(
                    expression,
                    new DeleteOptions
                    {

                    },
                    cancellationToken
                );

                if (deletion.IsAcknowledged)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            return;
        }

        public async Task RemoveRangeAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            DeleteResult deletion = default;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                deletion = await _collection.DeleteManyAsync(
                    expression,
                    new DeleteOptions
                    {
                        
                    },
                    cancellationToken
                );

                if (deletion.IsAcknowledged)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            return;
        }

        public async Task<T> UpdateAsync(Expression<Func<T, bool>> expression, T entity, CancellationToken cancellationToken = default)
        {
            var update = new BsonDocument { { "$set", entity.ToBsonDocument() } };
            var options = new FindOneAndUpdateOptions<T, T> { ReturnDocument = ReturnDocument.After };
            return await _collection.FindOneAndUpdateAsync(expression, update, options, cancellationToken);
        }

        public async Task<IEnumerable<T>> UpdateRangeAsync(Expression<Func<T, bool>> expression, T entity, CancellationToken cancellationToken = default)
        {
            var tasks = (await FindRangeAsync(expression, cancellationToken))
                .Select(async e =>
                    await UpdateAsync(x => x.Id == e.Id, entity, cancellationToken)
                );

            return await Task.WhenAll(tasks);
        }

        public void Dispose()
        {
        }
    }
}
