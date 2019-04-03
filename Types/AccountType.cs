using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.Types
{
    public class AccountType : ObjectGraphType<Account>
    {
        public AccountType(IRepository repository)
        {
            Field(x => x.Id, type: typeof(NonNullGraphType<IdGraphType>));
            Field(x => x.DisplayName);

            Connection<BroadcastType>()
                .Name("broadcasts")
                .Description("Gets pages of broadcasts hosted by the account.")
                .Bidirectional()
                .Resolve(resolver: context => 
                    repository.Connection<Broadcast, Account>(x => x.BroadcasterId == context.Source.Id, context));
        }
    }
}