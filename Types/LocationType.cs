using Gateway.Models;
using GraphQL.Types;

namespace Gateway.Types
{
    public class LocationType : ObjectGraphType<Location>
    {
        public LocationType() {
            Field(x => x.Longtitude);
            Field(x => x.Latitude);
        }
    }

    public class LocationInputType : InputObjectGraphType<Location>
    {
        public LocationInputType() {
            Field(x => x.Longtitude);
            Field(x => x.Latitude);
        }
    }
}