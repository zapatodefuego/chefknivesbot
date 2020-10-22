using ChefKnivesBot.Wiki;
using Reddit.Controllers;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Comment = Reddit.Controllers.Comment;

namespace ChefKnivesBot.Handlers.Comments
{
    public class WikifyCommentHandler : HandlerBase, ICommentHandler
    {
        private const string _urlRoute = "https://www.reddit.com";
        private readonly ILogger _logger;
        private readonly ISubredditService _service;

        public WikifyCommentHandler(ILogger logger , ISubredditService service, bool dryRun) 
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
        }

        public async Task<bool> Process(BaseController baseController, Func<string, Task> callback)
        {
            var comment = baseController as Comment;
            if (comment == null || !comment.Body.StartsWith("!wikify") || comment.Depth != 0)
            {
                return false;
            }

            if (!_service.Subreddit.Moderators.Any(m => m.Name.Equals(comment.Author)))
            {
                return false;
            }

            var result = await _service.SelfCommentDatabase.GetAny(nameof(SelfComment.ParentId), comment.Id);
            if (result != null)
            {
                return false;
            }

            if (!DryRun)
            {
                var commandParts = Regex.Split(comment.Body, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                if (commandParts.Length == 3)
                {
                    if (commandParts[1] == "review")
                    {
                        var knifeName = commandParts[2].Trim('"');
                        var post = _service.Subreddit.Post(comment.ParentFullname).Info();
                        var rewiewPage = new ReviewPage(_logger, _service);
                        rewiewPage.AddReviewLinkToReviewPage(knifeName, post.Author, post.Title, $"{_urlRoute}{post.Listing.Permalink}");

                        var replyComment = comment
                            .Reply("Added to the wiki: https://www.reddit.com/r/chefknives/wiki/reviews")
                            .Distinguish("yes");

                        _service.SelfCommentDatabase.Upsert(replyComment.ToSelfComment(comment.Id, RedditThingType.Comment, comment.Listing.AuthorFlairTemplateId));
                    }
                }
            }

            _logger.Information($"[{nameof(WikifyCommentHandler)}]: Commented on command: {comment.Body}");

            return true;
        }
    }
}
