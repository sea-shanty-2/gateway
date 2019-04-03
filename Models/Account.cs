using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gateway.Models
{
    public class Account : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id { get; set; }
        [BsonIgnoreIfDefault]
        public string DisplayName { get; set; }
        [BsonIgnoreIfDefault]
        public string FacebookId { get; set; }
    }
}