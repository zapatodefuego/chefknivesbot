using ChefKnivesBot.Lib.Data;
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

            if (comment.Depth == 0 && comment.Body.Equals("!review"))
            {
                _logger.Information($"Review invoked by {comment.Author} on post by {comment.Root.Author}");
            }
            else
            {
                return false;
            }

            var linkFlairId = comment.Root.Listing.LinkFlairTemplateId;
            if (linkFlairId != null &&
                linkFlairId.Equals(_makerPostFlair.Id))
            {
                var result = MakerCommentsReviewUtility.Review(comment.Root.Author, _service.RedditPostDatabase, _service.RedditCommentDatabase);

                if (!string.IsNullOrEmpty(result.Error))
                {
                    _logger.Error($"[{nameof(MakerPostReviewCommand)}:" + result.Error);
                    SendErrorMessage(comment);
                }

                if (result.OtherComments < 2)
                {
                    SendNeverContributedWarningMessage(comment, comment.Root);
                }
                else if (result.OtherComments < (result.SelfPostComments * 0.75))
                {
                    SendTenToOneWarningMessage(comment, result, comment.Root);
                }
                else
                {
                    SendGoodStandingMessage(comment);
                }

                return true;
            }

            return false;
        }

        private void SendErrorMessage(Comment comment)
        {
            if (!comment.Replies.Any(c => c.Author.Equals(_service.Account.Me.Name) && c.Body.StartsWith("Sorry,")))
            {
                if (!DryRun)
                {
                    comment
                    .Reply(
                        $"Sorry, I ran into an error because I'm not a very good bot...")
                    .Distinguish("yes");
                }

                _logger.Information($"Commented with SendGoodStandingMessage on comment by {comment.Author}. (Invoked by !review command)");
            }
        }

        private void SendGoodStandingMessage(Comment comment)
        {
            if (!comment.Replies.Any(c => c.Author.Equals(_service.Account.Me.Name) && c.Body.StartsWith("I reviewed OP and they appear to be in good standing.")))
            {
                if (!DryRun)
                {
                    comment
                        .Reply(
                            $"I reviewed OP and they appear to be in good standing.")
                        .Distinguish("yes");
                }

                _logger.Information($"Commented with SendGoodStandingMessage on comment by {comment.Author}. (Invoked by !review command)");
            }
        }

        private void SendNeverContributedWarningMessage(Comment comment, Post post)
        {
            if (!comment.Replies.Any(c => c.Author.Equals(_service.Account.Me.Name) && c.Body.StartsWith("I reviewd OP and they appear to not be in good standing.")))
            {
                if (!DryRun)
                {
                    comment
                        .Reply(
                            $"I reviewd OP and they appear to not be in good standing. u/{post.Author}, please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes");

                    //post.Remove();
                }

                _logger.Information($"Commented with SendNeverContributedWarningMessage on post by {post.Author}. (Invoked by !review command)");
            }
        }

        private void SendTenToOneWarningMessage(Comment comment, MakerReviewResult result, Post post)
        {
            if (!comment.Replies.Any(c => c.Author.Equals(_service.Account.Me.Name) && c.Body.StartsWith("I reviewd OP and they appear to not be in good standing. Of")))
            {
                if (!DryRun)
                {
                    comment
                        .Reply(
                            $"I reviewd OP and they appear to not be in good standing. Of their recent comments in r/{_service.Subreddit.Name}, {result.OtherComments} occured outside of their own posts " +
                            $"while {result.SelfPostComments} were made on posts they authored. \n\n " +
                            $"u/{post.Author}, please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes");

                    //post.Remove();
                }

                _logger.Information($"Commented with SendTenToOneWarningMessage on post by {post.Author}. (Invoked by !review command)");
            }
        }
    }
}
