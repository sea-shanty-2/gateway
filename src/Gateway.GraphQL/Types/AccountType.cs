using Gateway;
using Gateway.GraphQL.Extensions;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;


namespace Gateway.GraphQL.Types
{
    public class AccountType : ObjectGraphType<Account>
    {
        public AccountType(IRepository<Broadcast> broadcastRepository)
        {
            Field(x => x.Id);
            Field(x => x.DisplayName);

            Connection<BroadcastType>()
                .Name("broadcasts")
                .Description("Gets a page of broadcasts associated with the account.")
                .Bidirectional()
                .ResolveAsync(async context =>
                {
                    var entities = await broadcastRepository
                        .FindRangeAsync(x => x.AccountId == context.Source.Id, context.CancellationToken);
                    return entities.ToConnection(context);
                });
        }
    }
}