using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Queries
{
    public class AccountQuery : ObjectGraphType
    {
        public AccountQuery(Data data)
        {
            FieldAsync<AccountType>(
                "account",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "The ID of the account." }),
                resolve: async context => {
                    var id = context.GetArgument<string>("id");
                    return await data.Accounts.Find(x => x.Id == id).FirstOrDefaultAsync();
                }
            );

            FieldAsync<ListGraphType<AccountType>>(
                "accounts",
                resolve: async context => {
                    return await data.Accounts.Find(_ => true).ToListAsync();
                }
            );
        }
    }
}