using SubredditBot.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubredditBot.Lib.DataExtensions
{
    public static class RedditControllerPostDataExtensions
    {
        public static Post ToPost(this Reddit.Controllers.Post post)
        {
            return new Post
            {
                Author = post.Author,
                Permalink = post.Permalink,
                Title = post.Title,
                Flair = post.Listing.LinkFlairText,
                Id = post.Id,
                Fullname = post.Fullname,
                Kind = "t3",
                CreateDate = post.Created
            };
        }
    }
}
