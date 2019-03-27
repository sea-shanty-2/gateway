using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;

namespace Gateway.Types
{
    public class AccountGraphType : ObjectGraphType<Account>
    {
        public AccountGraphType(IRepository repository)
        {
            Field(x => x.Id, type: typeof(NonNullGraphType<IdGraphType>));
            Field(x => x.FirstName);
            Field(x => x.LastName);
            Field(x => x.FullName);

            Connection<BroadcastGraphType>()
                .Name("broadcasts")
                .Description("Gets pages of broadcasts hosted by the account.")
                .Bidirectional()
                .Resolve(resolver: context => 
                    repository.Connection<Broadcast, Account>(x => x.BroadcasterId == context.Source.Id, context));
        }
    }

    public class InputAccountGraphType : InputObjectGraphType<Account>
    {
        public InputAccountGraphType()
        {
            Field(x => x.FirstName);
            Field(x => x.LastName);
        }
    }
}