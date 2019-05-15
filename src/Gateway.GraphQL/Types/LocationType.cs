using Gateway.Models;
using GraphQL.Types;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Gateway.GraphQL.Types
{
    public class LocationType : ObjectGraphType<Location>
    {
        public LocationType() {
            Field<FloatGraphType>(
                "longitude",
                resolve: context => context.Source.Longitude.GetValueOrDefault()
            );
            Field<FloatGraphType>(
                "latitude",
                resolve: context => context.Source.Latitude.GetValueOrDefault()
            );
        }
    }

    public class LocationInputType : InputObjectGraphType<Location>
    {
        public LocationInputType() {
            Field<FloatGraphType>(
                "longitude",
                resolve: context => context.Source.Longitude.GetValueOrDefault()
            );
            Field<FloatGraphType>(
                "latitude",
                resolve: context => context.Source.Latitude.GetValueOrDefault()
            );
        }
    }
}