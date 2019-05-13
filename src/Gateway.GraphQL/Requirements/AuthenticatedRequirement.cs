using System.Threading.Tasks;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL.Authorization;

namespace Gateway.GraphQL.Requirements
{
    public class AuthenticatedRequirement : IAuthorizationRequirement
    {
        public Task Authorize(AuthorizationContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
                context.ReportError("Identity is unauthenticated");

            return Task.CompletedTask;
        }
    }
}