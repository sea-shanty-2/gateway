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
            Field(x => x.Categories, nullable: true);
            Field<NonNullGraphType<IntGraphType>>(
                "score",
                resolve: context => context.Source.Score.GetValueOrDefault()
            );

            FieldAsync<IntGraphType>(
                "rank",
                "rank of the account",
                resolve: async context => {
                    var id = context.Source.Id;

                    // TODO: Make efficient version, where it does not load into memory.
                    var query = await accountRepository.FindRangeAsync(x => true, context.CancellationToken);
                    
                    return query
                        .GroupBy(x => x.Score)
                        .OrderByDescending(g => g.First().Score)
                        .Select((group, i) => new {
                            Rank = i + 1,
                            Score = group.FirstOrDefault().Score,
                            Groups = group
                        })
                        .FirstOrDefault(group => group.Groups.FirstOrDefault(x => x.Id == id) != default)
                        .Rank;
                }
            );

            FieldAsync<FloatGraphType>(
                "percentile",
                "how many accounts above account in percent",
                resolve: async context => {
                    var accountScore = context.Source.Score; 
                    var accounts = await accountRepository.FindRangeAsync(x => true, context.CancellationToken);

                    // Have to be doubles
                    double greater = accounts.Count(a => a.Score > accountScore);
                    double total = accounts.Count();

                    return (greater / total) * 100;

                }
            );

            Connection<BroadcastType>()
                .Name("broadcasts")
                .Description("Gets a page of broadcasts associated with the account.")
                .Bidirectional()
                .ResolveAsync(async context =>
                {
                    var entities = (await broadcastRepository
                        .FindRangeAsync(x => x.AccountId == context.Source.Id, context.CancellationToken))
                        .OrderByDescending(x => x.Activity);

                    return entities.ToConnection(context);
                });
        }
    }
}