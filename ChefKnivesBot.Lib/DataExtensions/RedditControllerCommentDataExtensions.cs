using ChefKnivesBot.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChefKnivesBot.Lib.DataExtensions
{
    public static class RedditControllerCommentDataExtensions
    {
        public static Comment ToRedditComment(this Reddit.Controllers.Comment comment)
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
