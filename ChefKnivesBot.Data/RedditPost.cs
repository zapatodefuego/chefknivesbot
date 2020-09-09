namespace ChefKnivesBot.Data
{
    public class RedditPost
    {
        public string Id { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Flair { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Id == null ? 0 : Id.GetHashCode());
            hash = hash * 31 + (Author == null ? 0 : Author.GetHashCode());
            hash = hash * 31 + (Title == null ? 0 : Title.GetHashCode());
            hash = hash * 31 + (Flair == null ? 0 : Flair.GetHashCode());
            return hash;
        }

        public override bool Equals(object o)
        {
            if (o is RedditPost other)
            {
                return Id.Equals(other.Id)
                    && Author.Equals(other.Author)
                    && Title.Equals(other.Title)
                    && (Flair == null || Flair.Equals(other.Flair));
            }

            return false;
        }
    }
}