using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class ViewerType : ObjectGraphType<Viewer>
    {
        public ViewerType(IRepository<Account> accounts, IRepository<Broadcast> broadcasts)
        {
            FieldAsync<AccountType>(
                "account",
                "the viewers account",
                resolve: async context =>
                {
                    return await accounts.FindAsync(x => x.Id == context.Source.AccountId, context.CancellationToken);
                }
            );

            FieldAsync<BroadcastType>(
                "broadcast",
                "the viewed broadcast",
                resolve: async context =>
                {
                    return await broadcasts.FindAsync(x => x.Id == context.Source.BroadcastId, context.CancellationToken);
                }
            );

            Field(x => x.Timestamp);
        }
    }
}