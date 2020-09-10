using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChefKnivesBot.Data
{
    public abstract class RedditThing
    {
        public string Id { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.MinValue;

        public string Kind { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Id == null ? 0 : Id.GetHashCode());
            hash = hash * 31 + (Author == null ? 0 : Author.GetHashCode());
            hash = hash * 31 + (Kind == null ? 0 : Kind.GetHashCode());
            hash = hash * 31 + (CreateDate == null ? 0 : CreateDate.GetHashCode());
            return hash;
        }

        public override bool Equals(object o)
        {
            if (o is Post other)
            {
                return Id.Equals(other.Id)
                    && Author.Equals(other.Author)
                    && Kind.Equals(other.Kind)
                    && (CreateDate == null || CreateDate.Equals(other.CreateDate));
            }

            return false;
        }
    }
}
