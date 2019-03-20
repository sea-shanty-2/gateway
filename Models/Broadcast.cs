using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gateway.Models
{
    public class Broadcast
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }
    }
}