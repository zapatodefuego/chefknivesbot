namespace ChefKnivesBot.Data
{
    public class Comment : RedditThing
    {
        public string Body { get; set; } = string.Empty;

        public string PostLinkId { get; set; } = string.Empty;
    }

}