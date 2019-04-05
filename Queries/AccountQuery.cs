using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Gateway.Queries
{
    public class AccountQuery : ObjectGraphType<object>
    {
        public AccountQuery(IRepository repository)
        {
            //this.AuthorizeWith("AuthenticatedPolicy");
            
            this.FieldAsync<AccountType, Account>(
                "single",
                "Get an account by its unique identifier.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "id",
                        Description = "The unique identifier of the account.",
                    }),
                resolve: async resolveContext =>
                {

                    var id = resolveContext.GetArgument<string>("id");
                    return await repository.FindOneAsync<Account>(x => x.Id == id);
                });

            this.Connection<AccountType>()
                .Name("page")
                .Description("Gets pages of accounts.")
                .Bidirectional()
                .Resolve(resolveContext =>
                {

                    var values = resolveContext.UserContext;
                    return repository.Connection<Account, object>(_ => true, resolveContext);
                });
        }
    }
}