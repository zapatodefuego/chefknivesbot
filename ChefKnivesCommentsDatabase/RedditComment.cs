using ChefKnivesCommentsDatabase.Serialization;

namespace ChefKnivesCommentsDatabase
{
    public class RedditComment
    {
        public string Author { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string PostLinkId { get; set; } = string.Empty;

        public RedditComment() { }
        public RedditComment(RedditCommentExpanded expandedComment)
        {
            Author = expandedComment.author;
            Body = expandedComment.body;
            Id = expandedComment.id;
            PostLinkId = expandedComment.link_id;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Author == null ? 0 : Author.GetHashCode());
            hash = hash * 31 + (Body == null ? 0 : Body.GetHashCode());
            hash = hash * 31 + (Id == null ? 0 : Id.GetHashCode());
            hash = hash * 31 + (PostLinkId == null ? 0 : PostLinkId.GetHashCode());
            return hash;
        }

        public override bool Equals(object o)
        {
            if (o is RedditComment other)
            {
                return Author.Equals(other.Author) 
                    && Body.Equals(other.Body) 
                    && Id.Equals(other.Id) 
                    && PostLinkId.Equals(other.PostLinkId);
            }

            return false;
        }
    }

}