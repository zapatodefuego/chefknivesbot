namespace SubredditBot.Data
{
    public class Post : RedditThing
    {
        public string Title { get; set; } = string.Empty;

        public string Flair { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            hash = hash * 31 + (Title == null ? 0 : Title.GetHashCode());
            hash = hash * 31 + (Flair == null ? 0 : Flair.GetHashCode());
            return hash;
        }

        public override bool Equals(object o)
        {
            if (o is Post other)
            {
                return base.Equals(o) 
                    && Title.Equals(other.Title)
                    && Flair.Equals(other.Flair);
            }

            return false;
        }
    }
}