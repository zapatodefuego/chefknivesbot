namespace ChefKnivesBot.Data
{
    public class Post : RedditThing
    {
        public string Title { get; set; } = string.Empty;

        public string Flair { get; set; } = string.Empty;

    }
}