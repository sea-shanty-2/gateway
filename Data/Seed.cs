using System;
using System.Linq;
using Bogus;
using Gateway.Models;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Gateway.Data
{
    public static class Seed
    {
        public static void Run(IDatabase database)
        {
            var accounts = new Faker<Account>()
                .CustomInstantiator(f => new Account()
                {
                    DisplayName = f.Person.UserName
                }).Generate(10000);

            database.GetCollection<Account>().InsertMany(accounts);
            
            var broadcasts = new Faker<Broadcast>()
                .CustomInstantiator(f =>
                {
                    var started = f.Date.Recent();
                    return new Broadcast()
                    {
                        Title = f.Lorem.Sentence(),
                        Tag = f.Lorem.Word(),
                        BroadcasterId = accounts.Skip(f.Random.Int(0, accounts.Count() - 1)).FirstOrDefault()?.Id,
                        Started = started,
                        Ended = f.Random.Bool() ? started.AddMinutes(f.Random.Double(1, 360)) : default,
                        Location = GeoJson.Geographic(f.Random.Double(55.67549, 57.048), f.Random.Double(9.9187, 12.56553))
                    };
                });


            database.GetCollection<Broadcast>().InsertMany(broadcasts.Generate(100));
        }
    }
}