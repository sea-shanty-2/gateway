using Gateway.Models;
using GraphQL.Types;

namespace Gateway.Types
{
    public class AccountInputType : InputObjectGraphType<Account>
    {
        public AccountInputType()
        {
            Field(x => x.DisplayName);
        }
    }
}