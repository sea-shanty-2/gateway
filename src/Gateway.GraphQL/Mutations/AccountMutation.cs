using Gateway.GraphQL.Types;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;
using MongoDB.Bson;
using MongoDB.Driver;
using GraphQL.Authorization;
using GraphQL;

namespace Gateway.GraphQL.Mutations
{
    public class AccountMutation : ObjectGraphType<object>
    {
        public AccountMutation(IRepository<Account> repository)
        {
            this.FieldAsync<AccountType>(
                "me",
                "mutate the logged in account",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AccountUpdateInputType>>()
                    {
                        Name = "account"
                    }
                ),
                resolve: async context =>
                {
                    var id = context.UserContext.As<UserContext>().User.Identity.Name;

                    var account = await repository.FindAsync(x => x.Id == id);

                    if (account == default) {
                        context.Errors.Add(new ExecutionError($"Account {id} not found"));
                        return default;
                    }
                        
                    account = context.GetArgument<Account>("account");

                    return await repository.UpdateAsync(x => x.Id == id, account, context.CancellationToken);
                }
            ).AuthorizeWith("AuthenticatedPolicy");

            this.FieldAsync<AccountType>(
                "update",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<AccountUpdateInputType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");

                    var account = await repository.FindAsync(x => x.Id == id, context.CancellationToken);

                    if (account == default) {
                        context.Errors.Add(new ExecutionError($"Account {id} not found"));
                        return default;
                    }

                    var update = context.GetArgument<Account>("account");

                    if (update.Score != default)
                    {
                        update.Score += account.Score;
                    }

                    return await repository.UpdateAsync(x => x.Id == id, account, context.CancellationToken);
                }).AuthorizeWith("AuthenticatedPolicy");

            this.FieldAsync<IdGraphType>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    await repository.RemoveAsync(x => x.Id == id, context.CancellationToken);
                    return id;
                });
        }
    }
}