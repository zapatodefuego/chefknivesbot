using ChefKnivesBot.Utilities;
using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Lib;
using SubredditBot.Lib.Data;
using System;
using System.Linq;
using Post = Reddit.Controllers.Post;

namespace ChefKnivesBot.Handlers.Posts
{
    public class TenToOnePostHandler : HandlerBase, IPostHandler
    {
        private readonly ILogger _logger;
        private readonly ISubredditService _service;
        private readonly FlairV2 _makerPostFlair;

        public TenToOnePostHandler(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _makerPostFlair = _service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));
        }

        public bool Process(BaseController baseController)
        {
            var post = baseController as Post;
            if (post == null)
            {
                return false;
            }

            var linkFlairId = post.Listing.LinkFlairTemplateId;

            // Checkc that the tile contains [maker post] or that the link flair matches the maker post flair
            if (post.Title.Contains("[maker post]", StringComparison.OrdinalIgnoreCase)
                    || (linkFlairId != null && linkFlairId.Equals(_makerPostFlair.Id)))
            {
                var result = MakerCommentsReviewUtility.Review(post.Author, _service.RedditPostDatabase, _service.RedditCommentDatabase).Result;
                if (result.OtherComments < 2)
                {
                    SendNeverContributedWarningMessage(post);
                }
                else if (result.OtherComments < (result.SelfPostComments * 0.75))
                {
                    SendTenToOneWarningMessage(post, result);
                }

                return true;
            }

            return false;
        }

        private void SendNeverContributedWarningMessage(Post post)
        {
            if (!post.Comments.New.Any(c => c.Author.Equals(_service.Account.Me.Name) && c.Body.StartsWith("It looks like you haven't")))
            {
                if (!DryRun)
                {
                    post
                        .Reply(
                            $"It looks like you haven't recently contributed to this community. Please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes", false);

                    post.Remove();
                }

                _logger.Information($"Commented with SendNeverContributedWarningMessage on post by {post.Author}");
            }
        }

        private void SendTenToOneWarningMessage(Post post, MakerReviewResult result)
        {
            if (!post.Comments.New.Any(c => c.Author.Equals(_service.Account.Me.Name) && c.Body.StartsWith("Of your recent comments in")))
            {
                if (!DryRun)
                {
                    post
                        .Reply(
                            $"Of your recent comments in {_service.Subreddit.Name}, {result.OtherComments} occured outside of your own posts while {result.SelfPostComments} were made on posts your authored. \n\n " +
                            $"Please sufficiently interact with r/{_service.Subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes", false);

                    post.Remove();
                }

                _logger.Information($"Commented with SendTenToOneWarningMessage on post by {post.Author}. ");
            }
        }
    }
}
