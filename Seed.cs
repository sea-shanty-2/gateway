using System;
using System.Linq;
using Bogus;
using Gateway.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway
{
    public static class Seed
    {
        public static void SeedDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var data = serviceScope.ServiceProvider.GetService<Data>();
                SeedDatabase(data);
            }
        }

        private static void SeedDatabase(Data data) {
            Randomizer.Seed = new Random(8675309);
            
            if (data.GetCollection<Account>().EstimatedDocumentCount() <= 0) {
                data.GetCollection<Account>().InsertMany(Enumerable.Range(0, 100).Select(i => new Account()));
            }

            if (data.GetCollection<Broadcast>().EstimatedDocumentCount() <= 0) {
                var broadcastFactory = new Faker<Broadcast>()
                    .RuleFor(o => o.Name, f => f.Name.FullName())
                    .RuleFor(o => o.Token, f => f.UniqueIndex.ToString());
                
                data.GetCollection<Broadcast>().InsertMany(broadcastFactory.Generate(100));
            }
            
        }
    }
}