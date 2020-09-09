using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess.Serialization;

namespace ChefKnivesBot.DataAccess.DataExtensions
{
    public static class RedditCommentExpandedExtensions
    {
        public static RedditComment ToRedditComment(this RedditCommentExpanded expandedComment)
        {
            return new RedditComment
            {
                Author = expandedComment.author,
                Body = expandedComment.body,
                Id = expandedComment.id,
                PostLinkId = expandedComment.link_id,
            };
        }
    }
}
