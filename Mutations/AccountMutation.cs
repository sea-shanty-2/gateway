using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL.Types;
using MongoDB.Driver;
using Gateway.Extensions;

namespace Gateway.Mutations
{
    public class AccountMutation : ObjectGraphType<object>
    {
        public AccountMutation(IAccountRepository accounts)
        {

            this.FieldAsync<AccountGraphType, Account>(
                "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<InputAccountGraphType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var account = context.GetArgument<Account>("account");
                    return await accounts.CreateAsync(account, context.CancellationToken);
                });

            this.FieldAsync<AccountGraphType, Account>(
                "update",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    },
                    new QueryArgument<NonNullGraphType<InputAccountGraphType>>()
                    {
                        Name = "account"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    var account = context.GetArgument<Account>("account");
                    return await accounts.UpdateAsync(id, account, context.CancellationToken);
                });

            this.FieldAsync<AccountGraphType, Account>(
                "delete",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>()
                    {
                        Name = "id"
                    }),
                resolve: async context =>
                {
                    var id = context.GetArgument<string>("id");
                    return await accounts.DeleteAsync(id, context.CancellationToken);
                });
        }
    }
}