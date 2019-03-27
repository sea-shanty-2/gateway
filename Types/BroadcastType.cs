using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.Types
{
    public class BroadcastGraphType : ObjectGraphType<Broadcast>
    {
        public BroadcastGraphType(IRepository repository)
        {
            Field(x => x.Id, type: typeof(NonNullGraphType<IdGraphType>));
            Field(x => x.Title);
            Field(x => x.Tag);
            Field(x => x.Started, type: typeof(DateTimeGraphType));
            Field(x => x.Ended, type: typeof(DateTimeGraphType));
            
            FieldAsync<AccountGraphType>(
                "broadcaster",
                resolve: async context =>
                    await repository.SingleAsync<Account>(x => x.Id == context.Source.BroadcasterId, context.CancellationToken)
            );
        }
    }

}