using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL;
using GraphQL.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Gateway.GraphQL.Requirements
{
    public class ValidApiKeyRequirement : IAuthorizationRequirement
    {
        private IEnumerable<KeyValuePair<string, string>> clients;
        public ValidApiKeyRequirement(IEnumerable<KeyValuePair<string, string>> clients)
        {
            this.clients = clients;
        }

        public Task Authorize(AuthorizationContext context)
        {
            var headers = context.UserContext.As<UserContext>().Headers;
            var values = StringValues.Empty;
            var key = string.Empty;

            if (headers.TryGetValue("API_KEY", out values))
            {
                key = values.FirstOrDefault();
            }
            else
            {
                context.ReportError("The header 'API_KEY' was empty");
                return Task.CompletedTask;
            }

            if (clients == default)
            {
                context.ReportError($"No registered API clients found in the server configuration");
                return Task.CompletedTask;
            }

            if (!clients.Select(x => x.Value).Contains(key))
            {
                context.ReportError($"The API key '{key}' is unknown");
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}