using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gateway.Models
{
    public abstract class Entity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id { get; set; }
    }
}