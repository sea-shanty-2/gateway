

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;

namespace Gateway.Repositories
{
    public interface IRepository<T> where T : Entity
    {
        Task<T> GetAsync(string id, CancellationToken cancellationToken);
        IEnumerable<T> Get(CancellationToken cancellationToken);
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken);
        Task<T> UpdateAsync(string id, T entity, CancellationToken cancellationToken);
        Task<T> DeleteAsync(string id, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
    }
}