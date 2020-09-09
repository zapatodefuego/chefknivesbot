using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess.Serialization;

namespace ChefKnivesBot.DataAccess.DataExtensions
{
    public static class RedditPostExpandedExtensions
    {
        public static RedditPost ToRedditPost(this RedditPostExpanded expandedPost)
        {
            return new RedditPost
            {
                Id = expandedPost.id,
                Author = expandedPost.author,
                Title = expandedPost.title,
                Flair = expandedPost.link_flair_text
            };
        }
    }
}
