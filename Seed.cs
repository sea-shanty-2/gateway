using System;
using System.Linq;
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
            if (data.Accounts.EstimatedDocumentCount() <= 0) {
                data.Accounts.InsertMany(Enumerable.Range(0, 100).Select(i => new Account()));
            }
            
        }
    }
}