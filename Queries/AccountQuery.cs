using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;

namespace Gateway.Queries
{
    public class AccountQuery : ObjectGraphType<object>
    {
        public AccountQuery(IRepository repository)
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
                    return await repository.SingleAsync<Account>(x => x.Id == id);
                });

            this.Connection<AccountGraphType>()
                .Name("page")
                .Description("Gets pages of accounts.")
                .Bidirectional()
                .Resolve(context => {
                    return repository.Connection<Account, object>(_ => true, context);
                });
        }
    }
}