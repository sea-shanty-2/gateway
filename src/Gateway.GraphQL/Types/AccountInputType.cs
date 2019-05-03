using Gateway.Models;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class AccountInputType : InputObjectGraphType<Account>
    {
        public AccountInputType()
        {
            Field(x => x.DisplayName);
        }

    }
    
    public class AccountUpdateInputType : InputObjectGraphType<Account>
    {
        public AccountUpdateInputType()
        {
            Field(x => x.DisplayName, nullable: true);
            Field(x => x.Categories, nullable: true);
            Field(x => x.Score, nullable: true);
        }

    }

}