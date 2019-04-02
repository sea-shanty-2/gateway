using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Gateway.Controllers
{
    [Route("/")]
    public class GraphQLController : Controller
    {
        private readonly IDocumentExecuter executer;
        private readonly ISchema schema;

        public GraphQLController(IDocumentExecuter executer, ISchema schema)
        {
            this.executer = executer;
            this.schema = schema;
        }


        [HttpPost]
        public async Task<ActionResult<object>> PostAsync([FromBody]GraphQLQuery query)
        {
            if (query == null)
            {
                return BadRequest(new ArgumentNullException(nameof(query)));
            }

            var inputs = query.Variables.ToInputs();
            var queryToExecute = query.Query;
            var start = DateTime.UtcNow;

            var result = await executer.ExecuteAsync(o =>
            {
                o.Schema = schema;
                o.Query = queryToExecute;
                o.OperationName = query.OperationName;
                o.Inputs = inputs;
                o.ComplexityConfiguration = new GraphQL.Validation.Complexity.ComplexityConfiguration { MaxDepth = 15 };
                o.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                o.UserContext = HttpContext;
            }).ConfigureAwait(false);

            try
            {
                result.EnrichWithApolloTracing(start);
            }
            catch (InvalidOperationException)
            { }

            return this.Ok(result);
        }
    }

    public class GraphQLQuery
    {
        public string OperationName { get; set; }
        public string Query { get; set; }
        public JObject Variables { get; set; }
    }
}