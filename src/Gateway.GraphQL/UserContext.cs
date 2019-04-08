using System.Security.Claims;
using GraphQL.Authorization;

namespace Gateway
{
    public class UserContext : IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }
    }
}