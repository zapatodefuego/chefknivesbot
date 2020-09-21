namespace SubredditBot.Data
{
    public class SelfComment : Comment
    {
        public string ParentId { get; set; } = string.Empty;

        public RedditThingType ParentType { get; set; }

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            hash = hash * 31 + (ParentId == null ? 0 : ParentId.GetHashCode());
            hash = hash * 31 + ParentType.GetHashCode();
            return hash;
        }

        public override bool Equals(object o)
        {
            if (o is SelfComment other)
            {
                return base.Equals(o)
                    && Equals(ParentId, other.ParentId)
                    && Equals(ParentType, other.ParentType);
            }

            return false;
        }
    }
}
