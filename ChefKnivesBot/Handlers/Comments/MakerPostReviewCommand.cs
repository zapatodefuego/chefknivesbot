using SubredditBot.Data;
using SubredditBot.Lib.DataExtensions;
using SubredditBot.Lib.Utilities;
using Reddit.Controllers;
using Serilog;
using System.Linq;
using Comment = Reddit.Controllers.Comment;
using SubredditBot.Lib;

namespace ChefKnivesBot.Handlers.Comments
{
    public class MakerPostReviewCommand : HandlerBase, ICommentHandler
    {
        private ILogger _logger;
        private readonly SubredditService _service;

        public MakerPostReviewCommand(ILogger logger, SubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
        }

        public bool Process(BaseController baseController)
        {
            var comment = baseController as Comment;
            if (comment == null)
            {
                return false;
            }

            if (_service.SelfCommentDatabase.GetBy(nameof(SelfComment.ParentId), comment.Id).Result.Any())
            {
                _logger.Information($"[{nameof(MakerPostReviewCommand)}]: Comment {comment.Id} has already been replied to");
                return false;
            }


            if (comment.Depth == 0 && comment.Body.Equals("!review"))
            {
                _logger.Information($"[{nameof(MakerPostReviewCommand)}]: Review invoked by {comment.Author} on post by {comment.Root.Author}");
            }
            else
            {
                return false;
            }

            var result = MakerCommentsReviewUtility.Review(comment.Root.Author, _service.RedditPostDatabase, _service.RedditCommentDatabase).Result;
            if (!DryRun)
            {
                if (result.OtherComments < 2)
                {
                    var reply = comment
                        .Reply($"I reviewed OP and they appear to not have recently interacted with this community.")
                        .Distinguish("yes");

                    _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.Id, RedditThingType.Comment));
                }
                else if (result.OtherComments < (result.SelfPostComments * 0.75))
                {

                    var reply = comment
                        .Reply($"I reviewd OP and they appear to not be in good standing. u/{comment.Root.Author}, please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes");

                    _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.Id, RedditThingType.Comment));
                }
                else
                {
                    var reply = comment
                        .Reply(
                            $"I reviewd OP and they appear to not be in good standing. Of their recent comments in r/{_service.Subreddit.Name}, {result.OtherComments} occured outside of their own posts " +
                            $"while {result.SelfPostComments} were made on posts they authored. \n\n " +
                            $"u/{comment.Root.Author}, please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes");

                    _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.Id, RedditThingType.Comment));
                }
            }

            return true;
        }
    }
}
