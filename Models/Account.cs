using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gateway.Models
{
    public class Account : MongoUser, IEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => string.Join(' ', FirstName, LastName);
    }
}