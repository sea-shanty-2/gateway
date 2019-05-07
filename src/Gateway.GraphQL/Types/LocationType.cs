using Gateway.Models;
using GraphQL.Types;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Gateway.GraphQL.Types
{
    public class LocationType : ObjectGraphType<Location>
    {
        public LocationType() {
            Field(x => x.Longitude);
            Field(x => x.Latitude);
        }
    }

    public class LocationInputType : InputObjectGraphType<Location>
    {
        public LocationInputType() {
            
            Field(x => x.Longitude);
            Field(x => x.Latitude);
        }
    }
}