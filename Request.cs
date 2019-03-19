using Newtonsoft.Json.Linq;

namespace Gateway
{
    public class Request
    {
        public string OperationName { get; set; }
        public string Query { get; set; }
        public JObject Variables { get; set; }
    }
}