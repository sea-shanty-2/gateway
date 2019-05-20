using Gateway;
using Gateway.GraphQL.Types;
using Gateway.GraphQL.Extensions;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Gateway.GraphQL.Queries
{
    public class AccountQuery : ObjectGraphType<object>
    {
        public AccountQuery(IRepository<Account> repository)
        {
            this.FieldAsync<AccountType>(
                "me",
                "Get the logged in account",
                resolve: async context =>
                {
                    var identity = context.UserContext.As<UserContext>().User.Identity;
                    var account = await repository.FindAsync(x => x.Id == identity.Name, context.CancellationToken);

                    if (account == null)
                        context.Errors.Add(new ExecutionError($"Account {identity.Name} not found"));
                    
                    return account;
                }
            ).AuthorizeWith("AuthenticatedPolicy");

            this.FieldAsync<AccountType, Account>(
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
                    var account = await repository.FindAsync(x => x.Id == id, context.CancellationToken);
                    
                    if (account == null)
                        context.Errors.Add(new ExecutionError($"Account {id} not found"));

                    return account;
                });

            this.Connection<AccountType>()
                .Name("page")
                .Description("Gets pages of accounts.")
                .Bidirectional()
                .ResolveAsync(async context =>
                {
                    var entities = await repository.FindRangeAsync(_ => true, context.CancellationToken);
                    return entities.ToConnection(context);
                });
        }
    }
}