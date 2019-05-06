using System.Linq;
using Gateway;
using Gateway.GraphQL.Extensions;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Types;


namespace Gateway.GraphQL.Types
{
    public class AccountType : ObjectGraphType<Account>
    {
        public AccountType(IRepository<Broadcast> broadcastRepository, IRepository<Account> accountRepository)
        {
            Field(x => x.Id);
            Field(x => x.DisplayName);
            Field(x => x.Categories);
            Field(x => x.Score);

            FieldAsync<IntGraphType>(
                "rank",
                "rank of the account",
                resolve: async context => {
                    var id = context.Source.Id;

                    // TODO: Make efficient version, where it does not load into memory.
                    var query = await accountRepository.FindRangeAsync(x => true, context.CancellationToken);
                    
                    return query
                        .OrderByDescending(x => x.Score)
                        .GroupBy(x => x.Score)
                        .Select((group, i) => new {
                            Rank = i,
                            Groups = group
                        })
                        .FirstOrDefault(group => group.Groups.FirstOrDefault(x => x.Id == id) != default)
                        .Rank;
                }
            );

            Connection<BroadcastType>()
                .Name("broadcasts")
                .Description("Gets a page of broadcasts associated with the account.")
                .Bidirectional()
                .ResolveAsync(async context =>
                {
                    var entities = await broadcastRepository
                        .FindRangeAsync(x => x.AccountId == context.Source.Id, context.CancellationToken);
                    return entities.ToConnection(context);
                });
        }
    }
}