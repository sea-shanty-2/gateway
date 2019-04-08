using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Http;
using GraphQL.Instrumentation;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gateway.GraphQL
{
    public sealed class Middleware
    {
        private readonly RequestDelegate _next;
        private readonly Options _settings;
        private readonly IEnumerable<IValidationRule> _rules;
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;

        public Middleware(
            RequestDelegate next,
            Options settings,
            IEnumerable<IValidationRule> rules,
            IDocumentExecuter executer,
            IDocumentWriter writer)
        {
            _rules = rules;
            _next = next;
            _settings = settings;
            _executer = executer;
            _writer = writer;
        }

        public async Task InvokeAsync(HttpContext context, ISchema schema)
        {
            if (!context.Request.Path.StartsWithSegments(_settings.Path))
            {
                await _next(context);
                return;
            }

            // Handle requests as per recommendation at http://graphql.org/learn/serving-over-http/
            var httpRequest = context.Request;
            var request = new Request();

            if (HttpMethods.IsGet(httpRequest.Method) || (HttpMethods.IsPost(httpRequest.Method) && httpRequest.Query.ContainsKey(Request.QueryKey)))
                ExtractGraphQLRequestFromQueryString(httpRequest.Query, request);
            else if (HttpMethods.IsPost(httpRequest.Method))
            {
                if (!MediaTypeHeaderValue.TryParse(httpRequest.ContentType, out var mediaTypeHeader))
                {
                    await WriteBadRequestResponseAsync(context, _writer, $"Invalid 'Content-Type' header: value '{httpRequest.ContentType}' could not be parsed.");
                    return;
                }

                switch (mediaTypeHeader.MediaType)
                {
                    case "application/json":
                        request = Deserialize<Request>(httpRequest.Body);
                        break;
                    case "application/graphql":
                        request.Query = await ReadAsStringAsync(httpRequest.Body);
                        break;
                    case "application/x-www-form-urlencoded":
                        var formCollection = await httpRequest.ReadFormAsync();
                        ExtractGraphQLRequestFromPostBody(formCollection, request);
                        break;
                    default:
                        await WriteBadRequestResponseAsync(context, _writer, "Invalid 'Content-Type' header: non-supported media type. See: http://graphql.org/learn/serving-over-http/.");
                        return;
                }
            }
            else
                await WriteBadRequestResponseAsync(context, _writer, $"Invalid request method: value '{httpRequest.Method}' could not be handled. See: http://graphql.org/learn/serving-over-http/.");

            var start = DateTime.UtcNow;
            var result = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = request?.Query;
                _.OperationName = request?.OperationName;
                _.Inputs = request?.Variables.ToInputs();
                _.UserContext = new UserContext { User = context.User };
                _.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                _.ValidationRules = DocumentValidator.CoreRules().Concat(_rules);
                _.EnableMetrics = _settings.EnableMetrics;
                _.ExposeExceptions = _settings.ExposeExceptions;
            });

            if (_settings.EnableMetrics && result.Errors == null)
                result.EnrichWithApolloTracing(start);

            await WriteResponseAsync(context, result);
        }


        private async Task WriteResponseAsync(HttpContext context, ExecutionResult result)
        {
            var json = await _writer.WriteToStringAsync(result);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = result.Errors?.Any() == true ? (int)HttpStatusCode.BadRequest : (int)HttpStatusCode.OK;

            await context.Response.WriteAsync(json);
        }

        private Task WriteBadRequestResponseAsync(HttpContext context, IDocumentWriter writer, string errorMessage)
        {
            var result = new ExecutionResult()
            {
                Errors = new ExecutionErrors()
                {
                    new ExecutionError(errorMessage)
                }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 400; // Bad Request

            return writer.WriteAsync(context.Response.Body, result);
        }

        private Task WriteResponseAsync(HttpContext context, IDocumentWriter writer, ExecutionResult result)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200; // OK

            return writer.WriteAsync(context.Response.Body, result);
        }

        private static T Deserialize<T>(Stream s)
        {
            using (var reader = new StreamReader(s))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return new JsonSerializer().Deserialize<T>(jsonReader);
            }
        }

        private static async Task<string> ReadAsStringAsync(Stream s)
        {
            using (var reader = new StreamReader(s))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static void ExtractGraphQLRequestFromQueryString(IQueryCollection qs, Request gqlRequest)
        {
            gqlRequest.Query = qs.TryGetValue(Request.QueryKey, out var queryValues) ? queryValues[0] : null;
            gqlRequest.Variables = qs.TryGetValue(Request.VariablesKey, out var variablesValues) ? JObject.Parse(variablesValues[0]) : null;
            gqlRequest.OperationName = qs.TryGetValue(Request.OperationNameKey, out var operationNameValues) ? operationNameValues[0] : null;
        }

        private static void ExtractGraphQLRequestFromPostBody(IFormCollection fc, Request gqlRequest)
        {
            gqlRequest.Query = fc.TryGetValue(Request.QueryKey, out var queryValues) ? queryValues[0] : null;
            gqlRequest.Variables = fc.TryGetValue(Request.VariablesKey, out var variablesValue) ? JObject.Parse(variablesValue[0]) : null;
            gqlRequest.OperationName = fc.TryGetValue(Request.OperationNameKey, out var operationNameValues) ? operationNameValues[0] : null;
        }
    }
}