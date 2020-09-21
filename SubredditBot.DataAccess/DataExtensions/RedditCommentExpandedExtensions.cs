using SubredditBot.Data;
using SubredditBot.DataAccess.Serialization;
using System;

namespace SubredditBot.DataAccess.DataExtensions
{
    public static class RedditCommentExpandedExtensions
    {    
        public static Data.Comment ToComment(this RedditCommentExpanded expandedComment, string kind)
        {
            return new Data.Comment
            {
                Author = expandedComment.author,
                Body = expandedComment.body,
                Id = expandedComment.id,
                Fullname = expandedComment.name,
                PostLinkId = expandedComment.link_id,
                CreateDate = DatabaseConstants.EpochTime.AddSeconds(Convert.ToInt64(expandedComment.created_utc.Substring(0, expandedComment.created_utc.Length - 2))),
                Kind = kind
            };
        }
    }
}
