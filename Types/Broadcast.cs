using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gateway.Models;
using GraphQL.Language.AST;
using GraphQL.Types;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Gateway.Types
{
    public class BroadcastGraphType : ObjectGraphType<Broadcast>
    {
        public BroadcastGraphType()
        {
            Field(x => x.Id).Description("The id of the stream");
            Field(x => x.Name).Description("The name of the stream");
            Field(x => x.Token).Description("The token of the stream");

        }
    }

    public class BroadcastInputGraphType : InputObjectGraphType<Broadcast>
    {
        public BroadcastInputGraphType()
        {
            Field(x => x.Name).Description("The name of the stream");
            Field(x => x.Token).Description("The token of the stream");
        }
    }

}