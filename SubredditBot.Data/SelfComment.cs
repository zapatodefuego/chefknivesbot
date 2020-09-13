namespace SubredditBot.Data
{
    public class SelfComment : Comment
    {
        public string ParentId { get; set; } = string.Empty;

        public RedditThingType ParentType { get; set; }
    }
}
