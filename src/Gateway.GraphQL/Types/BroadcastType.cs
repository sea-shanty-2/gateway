using System;
using System.Linq;
using System.Collections.Generic;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;

namespace Gateway.GraphQL.Types
{
    public class BroadcastType : ObjectGraphType<Broadcast>
    {
        public BroadcastType(IRepository<Account> accounts, IRepository<Viewer> viewers)
        {
            Field(x => x.Id);
            Field(x => x.Location, type: typeof(LocationType));
            Field(x => x.Activity, type: typeof(DateTimeGraphType));
            Field(x => x.Categories);

            Field(x => x.JoinedTimeStamps, type: typeof(ListGraphType<ViewerDateTimePairType>))
                .DeprecationReason("start using 'viewers' instead, this is removed in the coming update");
            
            Field(x => x.LeftTimeStamps, type: typeof(ListGraphType<ViewerDateTimePairType>))
                .DeprecationReason("start using 'viewers' instead, this is removed in the coming update");

            Field(x => x.Reports);
            Field(x => x.PositiveRatings);
            Field(x => x.NegativeRatings);

            FieldAsync<ListGraphType<ViewerType>>(
                "viewers",
                "The viewers associated with this broadcast",
                resolve: async context => 
                    await viewers.FindRangeAsync(x => x.BroadcastId == context.Source.Id, context.CancellationToken)
            );

            FieldAsync<IntGraphType>(
                "viewer_count",
                "The number of viewers currently viewing this broadcast.",
                resolve: async context =>
                    (await viewers.FindRangeAsync(x => x.BroadcastId == context.Source.Id, context.CancellationToken))
                        .GroupBy(x => x.AccountId)
                        .Where(x => x.Count() % 2 != 0)
                        .Count(),
                deprecationReason: "'viewer_count' is changed to 'current_viewer_count' in the coming update"
            );

            FieldAsync<IntGraphType>(
                "current_viewer_count",
                "The number of viewers currently viewing this broadcast.",
                resolve: async context =>
                    (await viewers.FindRangeAsync(x => x.BroadcastId == context.Source.Id, context.CancellationToken))
                        .GroupBy(x => x.AccountId)
                        .Where(x => x.Count() % 2 != 0)
                        .Count()
            );

            FieldAsync<IntGraphType>(
                "total_viewer_count",
                "The total number of viewers that has viewed this broadcast.",
                resolve: async context =>
                    (await viewers.FindRangeAsync(x => x.BroadcastId == context.Source.Id, context.CancellationToken))
                        .GroupBy(x => x.AccountId)
                        .Count()
            );

            FieldAsync<AccountType>(
                "broadcaster",
                "the broadcast owner",
                resolve: async context =>
                    await accounts.FindAsync(x => x.Id == context.Source.AccountId, context.CancellationToken)
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

            Field<NonNullGraphType<StringGraphType>>(
                "rtmp",
                "the broadcast rtmp server",
                resolve: context =>
                    $"{configuration.GetValue<string>("RTMP_SERVER")}/{context.Source.Token}"
            );
        }
    }

    public class BroadcastStopType : ObjectGraphType<Broadcast>
    {
        public BroadcastStopType()
        {
            Field(x => x.JoinedTimeStamps, type: typeof(ListGraphType<ViewerDateTimePairType>));
            Field(x => x.LeftTimeStamps, type: typeof(ListGraphType<ViewerDateTimePairType>));
        }
    }
}