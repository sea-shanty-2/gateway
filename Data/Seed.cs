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
                }).Generate(100);

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
                        Ended = started.AddMinutes(f.Random.Double(1, 360)),
                        Location = GeoJson.Geographic(f.Random.Double(-180, 180), f.Random.Double(-90, 90))
                    };
                });


            database.GetCollection<Broadcast>().InsertMany(broadcasts.Generate(100));
        }
    }
}