
using GraphQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gateway
{
    public class Request
    {
        public const string QueryKey = "query";
        public const string VariablesKey = "variables";
        public const string OperationNameKey = "operationName";

        [JsonProperty(QueryKey)]
        public string Query { get; set; }

        [JsonProperty(VariablesKey)]
        public JObject Variables { get; set; }

        [JsonProperty(OperationNameKey)]
        public string OperationName { get; set; }

        public Inputs GetInputs()
        {
            return GetInputs(Variables);
        }

        public static Inputs GetInputs(JObject variables)
        {
            return variables?.ToInputs();
        }
    }
}