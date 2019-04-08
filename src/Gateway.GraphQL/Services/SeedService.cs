using Bogus;
using Gateway.Models;
using Gateway.Repositories;

namespace Gateway.GraphQL.Services
{
    public class SeedService
    {
        private readonly IRepository<Account> _accounts;

        public SeedService(IRepository<Account> accounts)
        {
            // Register injected repos
            _accounts = accounts;
        }

        public void Seed()
        {
            var accounts = new Faker<Account>()
                .CustomInstantiator(f => {
                    return new Account {
                        DisplayName = f.Person.UserName
                    };
                });

            _accounts.AddRangeAsync(accounts.Generate(10000));

        }
    }
}