using ChefKnivesBot.Utilities;
using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System.Linq;
using System.Threading.Tasks;
using Post = Reddit.Controllers.Post;

namespace ChefKnivesBot.Handlers.Posts
{
    public class RecommendMePostHandler : HandlerBase, IPostHandler
    {
        private const string _gettingStartedUrl = "https://www.reddit.com/r/chefknives/wiki/gettingstarted";
        private const string _questionnaireUrl = "https://www.reddit.com/r/chefknives/wiki/questionnaire";
        private ILogger _logger;
        private readonly ISubredditService _service;
        private readonly FlairV2 _flair;

        public RecommendMePostHandler(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _flair = service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Recommend me"));
        }

        public async Task<bool> Process(BaseController baseController)
        {
            var post = baseController as Post;
            if (post == null)
            {
                return false;
            }

            var linkFlairId = post.Listing.LinkFlairTemplateId;
            if (linkFlairId != null && linkFlairId.Equals(_flair.Id))
            {
                var existing = await _service.SelfCommentDatabase.GetAny(nameof(SelfComment.ParentId), post.Id);
                if (SelfPostUtilities.PostHasExistingResponse(existing, linkFlairId))
                {
                    return false;
                }

                if (!DryRun)
                {
                    var message = $"Please ensure you have filled out the Questionnaire and consider reviewing our Getting Started guide: \n\n" +
                    $"* Questionnaire: {_questionnaireUrl}\n" +
                    $"* Getting Started guide: {_gettingStartedUrl}\n\n" +
                    $"Low effort posts may be removed.";

                    var replyComment = post
                        .Reply(message)
                        .Distinguish("yes", true);

                    _service.SelfCommentDatabase.Upsert(replyComment.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));
                }

                _logger.Information($"[{nameof(RecommendMePostHandler)}]: Commented with recommend me details on post by {post.Author}");

                return true;
            }

            return false;
        }
    }
}
