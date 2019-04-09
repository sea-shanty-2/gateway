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
            ConventionRegistry.Register("IgnoreIfDefault",
                            new ConventionPack { new IgnoreIfDefaultConvention(true) },
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
            await _collection.InsertOneAsync(entity, null, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _collection.InsertManyAsync(entities, null, cancellationToken);
            return entities;
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            var entities = await _collection.FindAsync(expression, null, cancellationToken);
            return await entities.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> FindRangeAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken)
        {
            return await Task.Run(() => _collection.AsQueryable().Where(expression).AsEnumerable());
        }

        public Task RemoveAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _collection.DeleteOneAsync(expression, null, cancellationToken);
        }

        public Task RemoveRangeAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return _collection.DeleteManyAsync(expression, null, cancellationToken);
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
