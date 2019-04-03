using System.Security.Claims;
using GraphQL.Authorization;

namespace Gateway
{
    public class GraphQLUserContext : IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }
    }
}