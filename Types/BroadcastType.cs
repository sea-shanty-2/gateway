using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.Types
{
    public class BroadcastType : ObjectGraphType<Broadcast>
    {
        public BroadcastType(IRepository repository)
        {
            Field(x => x.Id);
            Field(x => x.Title);
            Field(x => x.Tag);
            Field(x => x.Location, type: typeof(LocationType));
            Field(x => x.Started, type: typeof(DateTimeGraphType));
            Field(x => x.Ended, type: typeof(DateTimeGraphType));

            FieldAsync<AccountType>(
                "broadcaster",
                resolve: async context =>
                    await repository.FindOneAsync<Account>(x => x.Id == context.Source.BroadcasterId, context.CancellationToken)
            );
        }
    }

    public class BroadcastCreateType : ObjectGraphType<Broadcast>
    {
        public BroadcastCreateType()
        {
            Field(x => x.Id);
            Field(x => x.Token);
        }
    }

    public class BroadcastDeleteType : ObjectGraphType<Broadcast>
    {
        public BroadcastDeleteType()
        {
            Field(x => x.Id);
        }
    }

    public class BroadcastUpdateType : ObjectGraphType<Broadcast>
    {
        public BroadcastUpdateType()
        {
            Field(x => x.Id);
            Field(x => x.Title);
        }
    }
}