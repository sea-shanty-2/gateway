namespace Gateway
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Gateway.Constants;
    using GraphQL;
    using GraphQL.Execution;
    using GraphQL.Instrumentation;
    using GraphQL.Server;
    using GraphQL.Server.Internal;
    using GraphQL.Types;
    using GraphQL.Validation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Headers;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    public class InstrumentingGraphQLExecutor<TSchema> : DefaultGraphQLExecuter<TSchema>
        where TSchema : ISchema
    {
        private readonly GraphQLOptions options;
        private readonly IHttpClientFactory clientFactory;

        public InstrumentingGraphQLExecutor(
            TSchema schema,
            IDocumentExecuter documentExecuter,
            IOptions<GraphQLOptions> options,
            IEnumerable<IDocumentExecutionListener> listeners,
            IEnumerable<IValidationRule> validationRules,
            IHttpClientFactory clientFactory)
            : base(schema, documentExecuter, options, listeners, validationRules)
        {
            this.options = options.Value;
            this.clientFactory = clientFactory;
        }


        public override async Task<ExecutionResult> ExecuteAsync(
            string operationName,
            string query,
            Inputs variables,
            object context,
            CancellationToken cancellationToken = default)
        {
            if (operationName != Operation.Introspection)
            {
                StringValues authHeader = "";
                context
                    .As<GraphQLUserContext>()
                    .Headers
                    .TryGetValue("Authorization", out authHeader);

                if (authHeader.DefaultIfEmpty() != default)
                {
                    /* var client = clientFactory.CreateClient();
                    await client.GetAsync($@"
                        graph.facebook.com/debug_token?
                            input_token ={authHeader.FirstOrDefault()}
                            &access_token ={ app - token - or - admin - token}
                    "); */

                }

            }

            var start = DateTime.UtcNow;
            var result = await base.ExecuteAsync(operationName, query, variables, context, cancellationToken);

            if (this.options.EnableMetrics)
            {
                try
                {
                    result.EnrichWithApolloTracing(start);
                }
                catch (InvalidOperationException)
                { }
            }

            return result;
        }

        protected override ExecutionOptions GetOptions(
            string operationName,
            string query,
            Inputs variables,
            object context,
            CancellationToken cancellationToken)
        {
            var options = base.GetOptions(operationName, query, variables, context, cancellationToken);

            if (this.options.EnableMetrics)
            {
                // Add instrumentation to measure how long field resolvers take to execute.
                options.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
            }

            return options;
        }
    }
}
