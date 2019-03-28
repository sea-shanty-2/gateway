using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gateway.Models
{
    public class Broadcast : Entity
    {
        public Broadcast()
        {
            Token = Guid.NewGuid().ToString("N");
        }

        public string Title { get; set; }
        public string Tag { get; set; }
        [BsonRequired]
        public string Token { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BroadcasterId { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
        public Location Location { get; set; }

    }
}