using Gateway.Models;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return await data.GetCollection<Account>().Find(x => x.Id == id).FirstOrDefaultAsync();
                }
            );

            FieldAsync<ListGraphType<AccountType>>(
                "accounts",
                resolve: async context =>
                {
                    return await data.GetCollection<Account>().Find(_ => true).ToListAsync();
                }
            );

            /* Connection<AccountType>()
                .Name("test")
                .Bidirectional()
                .ResolveAsync(async context => {
                    return await data.GetCollection<Account>().FindAsync<Account>(new BsonDocument());
                }); */
        }
    }
}