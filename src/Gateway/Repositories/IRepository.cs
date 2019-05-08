using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Linq.Expressions;
using Gateway.Models;

namespace Gateway.Repositories
{
    public interface IRepository<T> : IDisposable where T : IEntity
    {
        /// <summary>
        /// Adds an entity.
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Adds entities of the specified collection.
        /// </summary>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        /// <summary>
        /// Finds the first entity that matches with the supplied expression.
        /// </summary>
        Task<T> FindAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        /// <summary>
        /// Finds all the entities that match with the supplied expression.
        /// </summary>
        Task<IEnumerable<T>> FindRangeAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        /// <summary>
        /// Updates the first entity that matches with the supplied expression.
        /// </summary>
        Task<T> UpdateAsync(Expression<Func<T, bool>> expression, T entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Updates all the entities that match with the supplied expression.
        /// </summary>
        Task<IEnumerable<T>> UpdateRangeAsync(Expression<Func<T, bool>> expression, T entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Removes the first entity that matches with the supplied expression.
        /// </summary>
        Task RemoveAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        /// <summary>
        /// Removes all the entities that match with the supplied expression.
        /// </summary>
        Task RemoveRangeAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);

    }
}