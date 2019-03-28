using System;
using System.Linq;
using Bogus;
using Gateway.Models;
using MongoDB.Driver;

namespace Gateway.Data
{
    public static class Seed
    {
        public static void Run(IDatabase database)
        {
            var accounts = new Faker<Account>()
                .CustomInstantiator(f => new Account()
                {
                    FirstName = f.Person.FirstName,
                    LastName = f.Person.LastName
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
                        Ended = started.AddMinutes(f.Random.Double(1, 360))
                    };
                });


            database.GetCollection<Broadcast>().InsertMany(broadcasts.Generate(100));
        }
    }
}