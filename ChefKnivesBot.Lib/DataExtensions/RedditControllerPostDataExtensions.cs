using ChefKnivesBot.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChefKnivesBot.Lib.DataExtensions
{
    public static class RedditControllerPostDataExtensions
    {
        public static Post ToRedditPost(this Reddit.Controllers.Post post)
        {
            return new Post
            {
                Author = post.Author,
                Title = post.Title,
                Flair = post.Listing.LinkFlairText,
                Id = post.Id,
                Kind = "t3",
                CreateDate = post.Created
            };
        }
    }
}
