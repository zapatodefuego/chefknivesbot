using ChefKnivesBot.Utilities;
using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Post = Reddit.Controllers.Post;

namespace ChefKnivesBot.Handlers.Posts
{
    public class MakerPostHandler : HandlerBase, IPostHandler
    {
        private const string _postUrlFirstPart = "https://www.reddit.com/r/chefknives/comments/";
        private const string _makerPostName = "Maker Post";
        private readonly ILogger _logger;
        private readonly ISubredditService _service;
        private FlairV2 _makerPostFlair;

        public MakerPostHandler(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _makerPostFlair = _service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals(_makerPostName));
        }

        public async Task<bool> Process(BaseController baseController, Func<string, Task> _)
        {
            var post = baseController as Post;
            if (post == null)
            {
                return false;
            }

            var linkFlairId = post.Listing.LinkFlairTemplateId;

            // Check that the tile contains [maker post] or that the link flair matches the maker post flair (does the work for updates? i don't know yet)
            if (post.Title.Contains("[maker post]", StringComparison.OrdinalIgnoreCase) ||
                (linkFlairId != null && linkFlairId.Equals(_makerPostFlair.Id)))
            {
                // Set the flair
                post.SetFlair(_makerPostFlair.Text, _makerPostFlair.Id);

                // Check if we already commented on this post
                var existing = await _service.SelfCommentDatabase.GetAny(nameof(SelfComment.ParentId), post.Id);
                if (SelfPostUtilities.PostHasExistingResponse(existing, linkFlairId))
                {
                    return false;
                }

                var result = MakerCommentsReviewUtility.Review(post.Author, _service.RedditPostDatabase, _service.RedditCommentDatabase).Result;
                var nonMakerCommentCount = result.Comments.Count() - result.SelfComments.Count();
                var makerPostCount = result.MakerPosts.Count();

                if (nonMakerCommentCount < 2)
                {
                    SendNeverContributedWarningMessage(post);
                }
                else if (nonMakerCommentCount < makerPostCount * 3)
                {
                    SendTenToOneWarningMessage(post, nonMakerCommentCount, makerPostCount);
                }
                else
                {
                    SendMakerPostSticky(post);
                }

                return true;
            }

            return false;
        }

        private void SendNeverContributedWarningMessage(Post post)
        {
            if (!DryRun)
            {
                var reply = post
                    .Reply(
                        $"It looks like you haven't recently commented on others posts within this community. " +
                        $"Please sufficiently interact with r/{_service.Subreddit.Name} by constructively commenting on posts other than your own before submitting a Maker Post.\n\n" +
                        $"For more information review the [Maker FAQ](https://www.reddit.com/r/chefknives/wiki/makerfaq)")
                    .Distinguish("yes", false);

                _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));

                post.Remove();
            }

            _logger.Information($"Commented with SendNeverContributedWarningMessage on post by {post.Author}");
        }

        private void SendTenToOneWarningMessage(Post post, int nonMakerComments, int makerPostCount)
        {
            if (!DryRun)
            {
                var reply = post
                    .Reply(
                        $"It looks like you've submitted {makerPostCount} Maker Posts but only authored {nonMakerComments} comments outside of your own Maker Posts. " +
                        $"Please sufficiently interact with r/{_service.Subreddit.Name} by constructively commenting on posts other than your own before submitting a new Maker Post.\n\n" +
                        $"For more information review the [Maker FAQ](https://www.reddit.com/r/chefknives/wiki/makerfaq)")
                    .Distinguish("yes", false);

                _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));

                post.Remove();
            }

            _logger.Information($"Commented with SendTenToOneWarningMessage on post by {post.Author}.");
        }

        private void SendMakerPostSticky(Post post)
        {
            var replyMessage = new StringBuilder();
            replyMessage.AppendLine(
                "This post has been identified as a maker post! If you have not done so please review the [Maker FAQ](https://www.reddit.com/r/chefknives/wiki/makerfaq). \n\n " +
                "As a reminder to all readers, you may not discuss sales, pricing, for OP to make you something, or where to buy what OP is displaying or similar in this thread " +
                "or anywhere on r/chefknives. Use private messages for any such inquiries. \n\n");

            var postHistory = _service.RedditPostDatabase
                .GetByFilter(nameof(RedditThing.Author), post.Author).Result
                .Where(p => p.Flair != null && p.Flair.Equals(_makerPostName));
            if (postHistory != null && postHistory.Any())
            {
                replyMessage.AppendLine("---");
                replyMessage.AppendLine($"Here are some past Maker Posts from u/{post.Author}:");
                postHistory.Take(5).ToList()
                    .ForEach(p => replyMessage.AppendLine($"* [{p.Title}]({_postUrlFirstPart}{p.Id})"));
            }

            _logger.Information($"Commented with SendTenToOneWarningMessage on post by {post.Author}. ");

            if (!DryRun)
            {
                var reply = post
                    .Reply(replyMessage.ToString())
                    .Distinguish("yes", true);

                _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));
            }

            _logger.Information($"Commented with maker warning on post by {post.Author}");
        }
    }
}
