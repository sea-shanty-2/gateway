using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;
using Gateway.Extensions;

namespace Gateway.Queries
{
    public class AccountQuery : ObjectGraphType<object>
    {
        public AccountQuery(IAccountRepository accounts)
        {

            this.FieldAsync<AccountGraphType, Account>(
                "single",
                "Get an account by its unique identifier.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "id",
                        Description = "The unique identifier of the account.",
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return await accounts.GetAsync(id, context.CancellationToken);
                });

            this.Connection<AccountGraphType>()
                .Name("page")
                .Description("Gets pages of accounts.")
                .Bidirectional()
                // Set the maximum size of a page, use .ReturnAll() to set no maximum size.
                .Resolve(context => {
                    return context.GetPagedResults<Account>(accounts.Get(context.CancellationToken));
                });
        }
    }
}