
using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Conversion;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

namespace Gateway
{
    [Route("graphql")]
    public class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDocumentExecuter _documentExecutor;
        private readonly IDependencyResolver _resolver;
        
        public Controller(
            IDocumentExecuter documentExecuter,
            IDependencyResolver resolver)
        {
            _documentExecutor = documentExecuter;
            _resolver = resolver;
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Request query)
        {
            var start = DateTime.UtcNow;
            var inputs = query.Variables.ToInputs();

            var result = await _documentExecutor.ExecuteAsync(_ =>
            {
                _.UserContext = _resolver.Resolve<Data>();
                _.Schema = _resolver.Resolve<Schema>();
                _.Query = query.Query;
                _.EnableMetrics = true;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;
                _.FieldNameConverter = new CamelCaseFieldNameConverter();
            }).ConfigureAwait(false);

            if(result.Errors?.Count > 0)
            {
                return BadRequest(result.Errors);
            }

            result.EnrichWithApolloTracing(start);
            
            return Ok(result);
        }
    }
}