namespace SubredditBot.Data
{
    public class Comment : RedditThing
    {
        public string Body { get; set; } = string.Empty;

        public string PostLinkId { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            hash = hash * 31 + (Body == null ? 0 : Body.GetHashCode());
            hash = hash * 31 + (PostLinkId == null ? 0 : PostLinkId.GetHashCode());
            return hash;
        }

        public override bool Equals(object o)
        {
            if (o is Comment other)
            {
                return base.Equals(o)
                    && Equals(Body, other.Body)
                    && Equals(PostLinkId, other.PostLinkId);
            }

            return false;
        }
    }
}