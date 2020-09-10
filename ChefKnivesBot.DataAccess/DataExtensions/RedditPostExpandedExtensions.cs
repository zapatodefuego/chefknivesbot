using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess.Serialization;
using System;

namespace ChefKnivesBot.DataAccess.DataExtensions
{
    public static class RedditPostExpandedExtensions
    {
        public static Data.Post ToPost(this RedditPostExpanded expandedPost, string kind)
        {
            return new Data.Post
            {
                Id = expandedPost.id,
                Author = expandedPost.author,
                Title = expandedPost.title,
                Flair = expandedPost.link_flair_text,
                CreateDate = DatabaseConstants.EpochTime.AddSeconds(Convert.ToInt64(expandedPost.created_utc.Substring(0, expandedPost.created_utc.Length - 2))),
                Kind = kind
            };
        }
    }
}
