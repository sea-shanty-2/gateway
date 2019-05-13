using System.Security.Claims;
using GraphQL.Authorization;
using Microsoft.AspNetCore.Http;

namespace Gateway
{
    public class UserContext : IProvideClaimsPrincipal
    {
        public IHeaderDictionary Headers { get; set; }
        public ClaimsPrincipal User { get; set; }
    }
}