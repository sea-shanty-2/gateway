using System;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Gateway.GraphQL.Types
{
    public class BroadcastType : ObjectGraphType<Broadcast>
    {
        public BroadcastType(IRepository<Account> repository)
        {
            Field(x => x.Id);
            Field(x => x.Location, type: typeof(LocationType));
            Field(x => x.Activity, type: typeof(DateTimeGraphType));
            Field(x => x.Categories);
            FieldAsync<AccountType>(
                "broadcaster",
                "the broadcast owner",
                resolve: async context =>
                    await repository.FindAsync(x => x.Id == context.Source.AccountId, context.CancellationToken)
            );
        }
    }

    public class BroadcastCreateType : ObjectGraphType<Broadcast>
    {
        public BroadcastCreateType(IConfiguration configuration, IRepository<Account> repository)
        {
            Field(x => x.Id);
            Field(x => x.Location, type: typeof(LocationType));
            Field(x => x.Activity, type: typeof(DateTimeGraphType));
            Field(x => x.Categories);
            FieldAsync<AccountType>(
                "broadcaster",
                "the broadcast owner",
                resolve: async context =>
                    await repository.FindAsync(x => x.Id == context.Source.AccountId, context.CancellationToken)
            );

            Field<StringGraphType>(
                "rtmp", 
                "the broadcast rtmp server", 
                resolve: context =>
                    $"{configuration.GetValue<string>("RTMP_SERVER")}/{Guid.NewGuid().ToString("N")}"
            );
        }
    }
}