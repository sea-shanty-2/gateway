using Gateway;
using Gateway.Models;
using GraphQL.Types;


namespace Gateway.GraphQL.Types
{
    public class AccountType : ObjectGraphType<Account>
    {
        public AccountType()
        {
            Name = "Account";
            Field(x => x.Id);
            Field(x => x.DisplayName);
        }
    }
}