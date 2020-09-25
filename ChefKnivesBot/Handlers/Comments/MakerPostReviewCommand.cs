using ChefKnivesBot.Utilities;
using Reddit.Controllers;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Comment = Reddit.Controllers.Comment;

namespace ChefKnivesBot.Handlers.Comments
{
    public class MakerPostReviewCommand : HandlerBase, ICommentHandler
    {
        private ILogger _logger;
        private readonly ISubredditService _service;

        public MakerPostReviewCommand(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
        }

        public async Task<bool> Process(BaseController baseController, Func<string, Task> callback)
        {
            var comment = baseController as Comment;
            if (comment == null)
            {
                return false;
            }

            if (_service.SelfCommentDatabase.ContainsAny(nameof(SelfComment.ParentId), comment.Id).Result)
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
