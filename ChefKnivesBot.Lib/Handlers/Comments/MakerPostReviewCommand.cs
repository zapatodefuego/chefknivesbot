using ChefKnivesBot.Data;
using ChefKnivesBot.Lib.Data;
using ChefKnivesBot.Lib.DataExtensions;
using ChefKnivesBot.Lib.Utilities;
using Reddit;
using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using System.Linq;
using Account = Reddit.Controllers.Account;
using Comment = Reddit.Controllers.Comment;
using Post = Reddit.Controllers.Post;
using Subreddit = Reddit.Controllers.Subreddit;

namespace ChefKnivesBot.Lib.Handlers.Comments
{
    public class MakerPostReviewCommand : HandlerBase, IControllerHandler
    {
        private ILogger _logger;
        private readonly ChefKnivesService _service;
        private readonly FlairV2 _makerPostFlair;

        public MakerPostReviewCommand(ILogger logger, ChefKnivesService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _makerPostFlair = _service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));
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
                _logger.Information($"[{nameof(MakerPostReviewCommand)}]: Comment {comment.Id} was already been replied to");
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

            var linkFlairId = comment.Root.Listing.LinkFlairTemplateId;
            if (linkFlairId != null &&
                linkFlairId.Equals(_makerPostFlair.Id))
            {
                var reviewTask = MakerCommentsReviewUtility.Review(comment.Root.Author, _service.RedditPostDatabase, _service.RedditCommentDatabase);
                var result = reviewTask.GetAwaiter().GetResult();
                var post = comment.Root;

                if (!string.IsNullOrEmpty(result.Error))
                {
                    _logger.Error($"[{nameof(MakerPostReviewCommand)}:" + result.Error);
                    var reply = comment
                        .Reply($"Sorry, I ran into an error because I'm not a very good bot...")
                        .Distinguish("yes");

                    _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.Id, RedditThingType.Comment));
                    _logger.Information($"[{nameof(MakerPostReviewCommand)}]: Commented with SendGoodStandingMessage on comment by {comment.Author}. (Invoked by !review command)");
                }

                if (!DryRun)
                {
                    if (result.OtherComments < 2)
                    {
                        var reply = comment
                            .Reply($"I reviewed OP and they appear to be in good standing.")
                            .Distinguish("yes");

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.Id, RedditThingType.Comment));
                        _logger.Information($"[{nameof(MakerPostReviewCommand)}]: Commented with SendGoodStandingMessage on comment by {comment.Author}. (Invoked by !review command)");
                    }
                    else if (result.OtherComments < (result.SelfPostComments * 0.75))
                    {

                        var reply = comment
                            .Reply($"I reviewd OP and they appear to not be in good standing. u/{post.Author}, please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                            .Distinguish("yes");

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.Id, RedditThingType.Comment));
                        _logger.Information($"[{nameof(MakerPostReviewCommand)}]: Commented with SendNeverContributedWarningMessage on post by {post.Author}. (Invoked by !review command)");
                    }
                    else
                    {
                        var reply = comment
                            .Reply(
                                $"I reviewd OP and they appear to not be in good standing. Of their recent comments in r/{_service.Subreddit.Name}, {result.OtherComments} occured outside of their own posts " +
                                $"while {result.SelfPostComments} were made on posts they authored. \n\n " +
                                $"u/{post.Author}, please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                            .Distinguish("yes");

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.Id, RedditThingType.Comment));
                        _logger.Information($"[{nameof(MakerPostReviewCommand)}]: Commented with SendTenToOneWarningMessage on post by {post.Author}. (Invoked by !review command)");
                    }
                }

                return true;
            }

            return false;
        }
    }
}
