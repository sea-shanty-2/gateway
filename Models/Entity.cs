using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gateway.Models
{
    public interface IEntity
    {
        string Id { get; set; }
    }
}