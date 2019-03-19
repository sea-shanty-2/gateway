
using System.Threading.Tasks;
using GraphQL;
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
            var inputs = query.Variables.ToInputs();

            var result = await _documentExecutor.ExecuteAsync(_ =>
            {
                _.Schema = _resolver.Resolve<Schema>();
                _.Query = query.Query;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;
            }).ConfigureAwait(false);

            if(result.Errors?.Count > 0)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result);
        }
    }
}