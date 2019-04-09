using Gateway;
using Gateway.Models;
using GraphQL.Types;


namespace Gateway.GraphQL.Types
{
    public class AccountType : ObjectGraphType<Account>
    {
        public AccountType()
        {
            Field(x => x.Id);
            Field(x => x.DisplayName);
        }
    }
}