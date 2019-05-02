using System;
using System.Security.Principal;
using Gateway.Models;
using GraphQL.Types;

namespace Gateway.GraphQL.Types
{
    public class ViewerDateTimePairType : ObjectGraphType<ViewerDateTimePair>
    {
        public ViewerDateTimePairType()
        {
            Field(x => x.Id);
            Field(x => x.Time);
        }
    }
}