using Gateway.Models;
using GraphQL.Types;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Gateway.Types
{
    public class LocationType : ObjectGraphType<GeoJson2DGeographicCoordinates>
    {
        public LocationType() {
            Field(x => x.Longitude);
            Field(x => x.Latitude);
        }
    }

    public class LocationInputType : InputObjectGraphType<GeoJson2DGeographicCoordinates>
    {
        public LocationInputType() {
            Field(x => x.Longitude);
            Field(x => x.Latitude);
        }
    }
}