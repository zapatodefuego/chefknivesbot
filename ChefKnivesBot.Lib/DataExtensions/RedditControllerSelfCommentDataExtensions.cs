using ChefKnivesBot.Data;

namespace ChefKnivesBot.Lib.DataExtensions
{
    public static class RedditControllerSelfCommentDataExtensions
    {
        public static SelfComment ToSelfComment(this Reddit.Controllers.Comment comment, string parentId, RedditThingType parentType)
        {
            return new SelfComment
            {
                Author = comment.Author,
                Body = comment.Body,
                Id = comment.Id,
                Kind = "t1",
                PostLinkId = comment.Listing.LinkId,
                CreateDate = comment.Created,
                ParentId = parentId,
                ParentType = parentType
            };
        }
    }
}
