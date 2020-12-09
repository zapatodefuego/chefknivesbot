using SubredditBot.Data;

namespace SubredditBot.Lib.DataExtensions
{
    public static class RedditControllerSelfCommentDataExtensions
    {
        public static SelfComment ToSelfComment(this Reddit.Controllers.Comment comment, string parentId, RedditThingType parentType, string parentFlairId)
        {
            return new SelfComment
            {
                Author = comment.Author,
                Permalink = comment.Permalink,
                Body = comment.Body,
                Id = comment.Id,
                Fullname = comment.Fullname,
                Kind = "t1",
                PostLinkId = comment.Listing.LinkId,
                CreateDate = comment.Created,
                ParentId = parentId,
                ParentType = parentType,
                ParentFlairId = parentFlairId
            };
        }
    }
}
