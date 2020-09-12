using SubredditBot.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubredditBot.Lib.DataExtensions
{
    public static class RedditControllerCommentDataExtensions
    {
        public static Comment ToComment(this Reddit.Controllers.Comment comment)
        {
            return new Comment
            {
                Author = comment.Author,
                Body = comment.Body,
                Id = comment.Id,
                Kind = "t1",
                PostLinkId = comment.Listing.LinkId,
                CreateDate = comment.Created
            };
        }
    }
}
