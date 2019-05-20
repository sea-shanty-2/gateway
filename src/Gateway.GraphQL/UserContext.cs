using System.Security.Claims;
using GraphQL.Authorization;
using Microsoft.AspNetCore.Http;

namespace Gateway
{
    public class UserContext : IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }
    }
}