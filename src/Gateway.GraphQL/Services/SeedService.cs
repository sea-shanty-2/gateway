using System.Collections.Generic;
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
              DisplayName = f.Person.UserName,
              Categories = new double[2]{f.Random.Double(0, 1), f.Random.Double(0, 1)},
              Score = f.Random.Int(0, 500000)
            };
          });

      _accounts.AddRangeAsync(accounts.Generate(10000));

      var broadcasts = new Faker<Broadcast>()
          .CustomInstantiator(f =>
          {
            return new Broadcast
            {
              Activity = f.Date.Soon(),
              Bitrate = f.Random.Int(0, 160),
              Location = new Location
              {
                Latitude = f.Random.Double(55.676098, 57.048820),
                Longitude = f.Random.Double(9.921747, 12.568337)
              },
              Stability = f.Random.Float(),
              Categories = new List<double> { f.Random.Double() }.ToArray()
            };

          });

      _broadcasts.AddRangeAsync(broadcasts.Generate(100));

    }
  }
}