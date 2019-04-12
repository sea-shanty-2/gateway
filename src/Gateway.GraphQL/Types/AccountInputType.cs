using Gateway.Models;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class AccountInputType : InputObjectGraphType<Account>
    {
        public AccountInputType()
        {
            Name = "AccountInput";
            Field(x => x.DisplayName);
        }
    }
}