using Gateway.Models;
using GraphQL.Types;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Gateway.GraphQL.Types
{
    public class LocationType : ObjectGraphType<Location>
    {
        public LocationType() {
            Name = "Location";
            Field(x => x.Longitude);
            Field(x => x.Latitude);
        }
    }

    public class LocationInputType : InputObjectGraphType<Location>
    {
        public LocationInputType() {
            Name = "LocationInput";
            Field(x => x.Longitude);
            Field(x => x.Latitude);
        }
    }
}