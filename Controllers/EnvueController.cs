
using System.Threading.Tasks;
using Gateway.Mutation;
using Gateway.Query;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("graphql")]
    public class EnvueController : Controller
    {
        private readonly IDocumentExecuter _documentExecutor;
        private readonly EnvueQuery _envueQuery;
        private readonly EnvueMutation _envueMutation;
        public EnvueController(
            IDocumentExecuter documentExecuter,
            EnvueQuery envueQuery,
            EnvueMutation envueMutation)
        {
            _documentExecutor = documentExecuter;
            _envueQuery = envueQuery;
            _envueMutation = envueMutation;
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query)
        {
            var inputs = query.Variables.ToInputs();

            var schema = new Schema()
            {
                Query = _envueQuery,
                Mutation = _envueMutation
            };

            var result = await _documentExecutor.ExecuteAsync(_ =>
            {
                _.Schema = schema;
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