using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess.Serialization;
using System;

namespace ChefKnivesBot.DataAccess.DataExtensions
{
    public static class RedditCommentExpandedExtensions
    {
        

        public static RedditComment ToRedditComment(this RedditCommentExpanded expandedComment, string kind)
        {
            return new RedditComment
            {
                Author = expandedComment.author,
                Body = expandedComment.body,
                Id = expandedComment.id,
                PostLinkId = expandedComment.link_id,
                CreateDate = DatabaseConstants.EpochTime.AddSeconds(Convert.ToInt64(expandedComment.created_utc.Substring(0, expandedComment.created_utc.Length - 2))),
                Kind = kind
            };
        }
    }
}
