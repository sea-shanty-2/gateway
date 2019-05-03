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
                    var account = context.GetArgument<Account>("account");
                    var identity = context.UserContext.As<UserContext>().User.Identity;

                    if (id != identity.Name)
                    {
                        context.Errors.Add(new ExecutionError("Not authorized to update account"));
                        return default;
                    }

                    if (account.Score != default)
                    {
                        var response = await 
                            repository.FindAsync(x => x.Id == id, context.CancellationToken);
                        
                        account.Score += response.Score;
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