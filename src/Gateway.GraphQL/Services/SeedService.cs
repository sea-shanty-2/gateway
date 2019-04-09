using Bogus;
using Gateway.Models;
using Gateway.Repositories;

namespace Gateway.GraphQL.Services
{
  public class SeedService
  {
    private readonly IRepository<Account> _accounts;
    private readonly IRepository<Broadcast> _broadcasts;

    public SeedService(IRepository<Account> accounts, IRepository<Broadcast> broadcasts)
    {
      // Register injected repos
      _accounts = accounts;
      _broadcasts = broadcasts;
    }

    public void Seed()
    {
      var accounts = new Faker<Account>()
          .CustomInstantiator(f =>
          {
            return new Account
            {
              DisplayName = f.Person.UserName
            };
          });

      _accounts.AddRangeAsync(accounts.Generate(10000));

      var broadcasts = new Faker<Broadcast>()
          .CustomInstantiator(f =>
          {
            return new Broadcast(f.Random.Guid().ToString())
            {
              Activity = f.Date.Soon(),
              Bitrate = f.Random.Int(0, 160),
              Location = new Location
              {
                Latitude = f.Random.Double(55.676098, 57.048820),
                Longitude = f.Random.Double(9.921747, 12.568337)
              },
              Stability = f.Random.Float()
            };

          });

      _broadcasts.AddRangeAsync(broadcasts.Generate(100));

    }
  }
}