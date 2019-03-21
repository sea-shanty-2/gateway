using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gateway.Models
{
    public class Broadcast : Entity
    {
        public string Name { get; set; }

        public string Token { get; set; }
    }
}