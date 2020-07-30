using ChefKnivesBotLib.Utilities;
using ChefKnivesBotLib.Data;
using Reddit;
using Reddit.Things;
using Serilog;
using System.Linq;
using Comment = Reddit.Controllers.Comment;
using Post = Reddit.Controllers.Post;
using Subreddit = Reddit.Controllers.Subreddit;

namespace ChefKnivesBotLib.Handlers.Comments
{
    public class MakerPostReviewCommand : ICommentHandler
    {
        private ILogger _logger;
        private readonly RedditClient _redditClient;
        private Subreddit _subreddit;
        private readonly Reddit.Controllers.User _me;
        private FlairV2 _makerPostFlair;

        public MakerPostReviewCommand(ILogger logger, RedditClient redditClient, Subreddit subreddit, Reddit.Controllers.User me)
        {
            _logger = logger;
            _redditClient = redditClient;
            _subreddit = subreddit;
            _me = me;
            _makerPostFlair = _subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));
        }

        public void Process(Comment comment)
        {
            if (comment.Depth == 0 && comment.Body.Equals("!review"))
            {
                _logger.Information($"Review invoked by {comment.Author} on post by {comment.Root.Author}");
            }
            else
            {
                return;
            }

            var linkFlairId = comment.Root.Listing.LinkFlairTemplateId;
            if (linkFlairId != null && 
                linkFlairId.Equals(_makerPostFlair.Id))
            {
                var result = MakerCommentsReviewUtility.Review(comment.Root.Author, _subreddit.Name, _redditClient);

                if (result.OtherComments < 3)
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
            }
        }

        private void SendGoodStandingMessage(Comment comment)
        {
            if (!comment.Replies.Any(c => c.Author.Equals(_me.Name) && c.Body.StartsWith("I reviewed OP and they appear to be in good standing.")))
            {
                comment
                    .Reply(
                        $"I reviewed OP and they appear to be in good standing.")
                    .Distinguish("yes");

                _logger.Information($"Commented with SendGoodStandingMessage on comment by {comment.Author}. (Invoked by !review command)");
            }
        }

        private void SendNeverContributedWarningMessage(Comment comment, Post post)
        {
            if (!comment.Replies.Any(c => c.Author.Equals(_me.Name) && c.Body.StartsWith("I reviewd OP and they appear to not be in good standing.")))
            {
                comment
                    .Reply(
                        $"I reviewd OP and they appear to not be in good standing. u/{post.Author}, please sufficiently interact with r/{_subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                    .Distinguish("yes");

                //post.Remove();

                _logger.Information($"Commented with SendNeverContributedWarningMessage on post by {post.Author}. (Invoked by !review command)");
            }
        }

        private void SendTenToOneWarningMessage(Comment comment, MakerReviewResult result, Post post)
        {
            if (!comment.Replies.Any(c => c.Author.Equals(_me.Name) && c.Body.StartsWith("I reviewd OP and they appear to not be in good standing. Of")))
            {
                comment
                    .Reply(
                        $"I reviewd OP and they appear to not be in good standing. Of their recent comments in r/{_subreddit.Name}, {result.OtherComments} occured outside of their own posts " +
                        $"while {result.SelfPostComments} were made on posts they authored. \n\n " +
                        $"u/{post.Author}, please sufficiently interact with r/{_subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                    .Distinguish("yes");

                //post.Remove();

                _logger.Information($"Commented with SendTenToOneWarningMessage on post by {post.Author}. (Invoked by !review command)");
            }
        }
    }
}
