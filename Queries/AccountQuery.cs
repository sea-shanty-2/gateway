using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Queries
{
    public class AccountQuery : ObjectGraphType
    {
        public AccountQuery(Data data)
        {
            FieldAsync<ListGraphType<AccountType>>(
                "accounts",
                resolve: async context => {
                    return await data.Accounts.Find(_ => true).ToListAsync();
                }
            );
        }
    }
}