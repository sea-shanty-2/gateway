using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Gateway.Interfaces;

namespace Gateway.Models
{
    public class Entity : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}