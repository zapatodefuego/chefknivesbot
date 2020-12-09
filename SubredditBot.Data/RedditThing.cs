using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SubredditBot.Data
{
    public abstract class RedditThing
    {
        public string Id { get; set; } = string.Empty;

        public string Fullname { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.MinValue;

        public string Kind { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public string Permalink { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Id == null ? 0 : Id.GetHashCode());
            hash = hash * 31 + (Author == null ? 0 : Author.GetHashCode());
            hash = hash * 31 + (Kind == null ? 0 : Kind.GetHashCode());
            hash = hash * 31 + (CreateDate == null ? 0 : CreateDate.GetHashCode());
            hash = hash * 31 + IsDeleted.GetHashCode();
            return hash;
        }

        public override bool Equals(object o)
        {
            if (o is RedditThing other)
            {
                return Equals(Id, other.Id)
                    && Equals(Author, other.Author)
                    && Equals(Kind, other.Kind)
                    && Equals(CreateDate, other.CreateDate)
                    && Equals(IsDeleted, other.IsDeleted);
            }

            return false;
        }
    }
}
