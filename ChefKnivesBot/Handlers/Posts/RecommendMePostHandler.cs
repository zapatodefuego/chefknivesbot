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
        private const string _recommendMeUrl = "https://www.reddit.com/r/chefknives/?f=flair_name%3A%22Recommend%20me%22";
        private const string _modMailUrl = "https://www.reddit.com/message/compose?to=%2Fr%2Fchefknives";
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
            "looking",
            "get",
            "getting",
            "recommendations",
            "thoughts",
            "need",
            "opinion",
            "opinions",
            "experience",
            "experiences",
            "want",
            "buying",
            "help",
            "good",
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
                    var message = $"This looks like a post asking for a knife-related recommendation. " +
                    $"You're in the right subreddit, but not the right thread. You'll want to redirect this question to the pinned weekly megathread. \n\n" +
                    $"* Please search through recent recommendations for similar requests and add a new comment if you don't find anything\n" + 
                    $"* Recent recommendation megathreads: {_recommendMeUrl}\n" +
                    $"* Getting Started guide: {_gettingStartedUrl}\n\n" +
                    $"This post was automatically removed - please notify [via modmail]({_modMailUrl}) if done in error (i.e. you're not asking for a recommendation) and it will be restored. ";

                    var replyComment = post
                        .Reply(message)
                        .Distinguish("yes", true);
                    _service.SelfCommentDatabase.Upsert(replyComment.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));

                    post.Remove();
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
