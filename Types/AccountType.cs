using Gateway.Models;
using GraphQL.Types;

namespace Gateway.Types
{
    public class AccountGraphType : ObjectGraphType<Account>
    {
        public AccountGraphType()
        {
            Field(x => x.Id, type: typeof(NonNullGraphType<IdGraphType>));
            Field(x => x.FirstName);
            Field(x => x.LastName);
            Field(x => x.FullName);
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