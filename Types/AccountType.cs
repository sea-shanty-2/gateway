using Gateway.Models;
using GraphQL.Types;

namespace Gateway.Types
{
    public class AccountType : ObjectGraphType<Account>
    {
        public AccountType()
        {
            Field(x => x.Id).Description("The id of the account");
        }
    }
}