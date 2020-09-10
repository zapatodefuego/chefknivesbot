using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChefKnivesBot.Data
{
    public abstract class RedditThing
    {
        public string Id { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.MinValue;
    }
}
