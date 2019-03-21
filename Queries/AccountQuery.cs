using Gateway.Extensions;
using Gateway.Models;
using Gateway.Types;
using GraphQL;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gateway.Queries
{
    public class AccountQuery : ObjectGraphType
    {
        public AccountQuery()
        {
            FieldAsync<AccountType>(
                "account",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "The ID of the account." },
                    new QueryArgument<StringGraphType> { Name = "name", Description = "The name of the account." }
                ),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return await context
                        .UserContext
                        .As<Data>()
                        .GetCollection<Account>()
                        .Find(x => x.Id == id)
                        .FirstOrDefaultAsync();
                }
            );

            Connection<AccountType>()
                .Name("accounts")
                .Bidirectional()
                .Resolve(context =>
                {
                    var data = context
                        .UserContext
                        .As<Data>()
                        .GetCollection<Account>()
                        .Find(_ => true)
                        .ToEnumerable();

                    return context.GetPagedResults<object, Account>(data);
                });
        }
    }
}