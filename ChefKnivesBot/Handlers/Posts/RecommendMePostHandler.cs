using ChefKnivesBot.Utilities;
using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Post = Reddit.Controllers.Post;

namespace ChefKnivesBot.Handlers.Posts
{
    public class RecommendMePostHandler : HandlerBase, IPostHandler
    {
        private const string _urlRoot = "https://www.reddit.com";
        private const string _gettingStartedUrl = "https://www.reddit.com/r/chefknives/wiki/gettingstarted";
        private const string _questionnaireUrl = "https://www.reddit.com/r/chefknives/?f=flair_name%3A%22Recommend%20me%22";
        private ILogger _logger;
        private readonly ISubredditService _service;
        private readonly FlairV2 _flair;
        private static readonly List<string> _recommendationKeyWords = new List<string>()
        {
            "budget",
            "buy",
            "recommend",
            "first",
            "purchase",
        };

        public RecommendMePostHandler(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _flair = service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Question") || f.Text.Equals("Discussion"));
        }

        public async Task<bool> Process(BaseController baseController, Func<string, Task> callback)
        {
            var post = baseController as Post;
            if (post == null)
            {
                return false;
            }

            var linkFlairId = post.Listing.LinkFlairTemplateId;
            if (linkFlairId != null && linkFlairId.Equals(_flair.Id) && IsSuspectedRecommendMePost(post))
            {
                var existing = await _service.SelfCommentDatabase.GetAny(nameof(SelfComment.ParentId), post.Id);
                if (SelfPostUtilities.PostHasExistingResponse(existing, linkFlairId))
                {
                    return false;
                }

                if (!DryRun)
                {
                    var message = $"This looks like a \"Recommend Me\" style post which is not allowed outside AutoModerator's weekly thread. \n\n" +
                    $"* Please search through recent recommendations for similar requests and add a new comment if you don't find anything\n" + 
                    $"* Recent recommendation megathreads: {_questionnaireUrl}\n" +
                    $"* Getting Started guide: {_gettingStartedUrl}\n\n" +
                    $"This post was automatically removed and a moderator will restore it if done in error. ";

                    var replyComment = post
                        .Reply(message)
                        .Distinguish("yes", true);
                    _service.SelfCommentDatabase.Upsert(replyComment.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));

                    post.Remove();

                    if (callback != null)
                    {
                        await callback($"Suspected \"Recommend Me\" post removed by {post.Author}, title: {_urlRoot}{post.Permalink}");
                    }
                }

                _logger.Information($"[{nameof(RecommendMePostHandler)}]: Commented with recommend me details on post by {post.Author}");

                return true;
            }

            return false;
        }

        private static bool IsSuspectedRecommendMePost(Post post)
        {
            if(_recommendationKeyWords.Any( 
                keyword => post.Title.Contains(keyword, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }
            return false;
        }
    }
}
