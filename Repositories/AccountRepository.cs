using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gateway.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IMongoCollection<Account> _accounts;

        public AccountRepository(IConfiguration config) {
            var client = new MongoClient(config.GetConnectionString("Envue"));
            var database = client.GetDatabase("envue");
            _accounts = database.GetCollection<Account>("Accounts");
        }

        public async Task<Account> CreateAsync(Account entity, CancellationToken cancellationToken)
        {
            await _accounts.InsertOneAsync(entity, cancellationToken: cancellationToken);
            return entity;
        }

        public async Task<Account> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            return await _accounts.FindOneAndDeleteAsync(x => x.Id == id, cancellationToken: cancellationToken);
        }

        public async Task<Account> GetAsync(string id, CancellationToken cancellationToken)
        {
            var accounts = await _accounts.FindAsync(x => x.Id == id, cancellationToken: cancellationToken);
            return await accounts.FirstOrDefaultAsync(cancellationToken);
        }

        public IEnumerable<Account> Get(CancellationToken cancellationToken)
        {
            return _accounts.Find(_ => true).ToEnumerable(cancellationToken);
        }

        public async Task<Account> UpdateAsync(string id, Account entity, CancellationToken cancellationToken)
        {
            var filter = Builders<Account>.Filter.Eq(x => x.Id, id);
            var update = entity.ToBsonDocument();
            return await _accounts.FindOneAndUpdateAsync<Account>(filter, update, cancellationToken: cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken) {
            return unchecked((int)await _accounts.EstimatedDocumentCountAsync(cancellationToken: cancellationToken));
        }
    }
}