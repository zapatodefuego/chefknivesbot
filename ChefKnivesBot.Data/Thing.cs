using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChefKnivesBot.Data
{
    public abstract class Thing
    {
        public string Id { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.MinValue;

        public string Kind { get; set; } = string.Empty;
    }
}
