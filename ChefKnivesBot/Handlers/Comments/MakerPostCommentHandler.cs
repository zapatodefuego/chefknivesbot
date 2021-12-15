using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comment = Reddit.Controllers.Comment;

namespace ChefKnivesBot.Handlers.Comments
{
    public class MakerPostCommentHandler : HandlerBase, ICommentHandler
    {
        private const string _urlRoot = "https://www.reddit.com";
        private static readonly List<string> _forbiddenPhrases = new List<string> { "buy", "sell", "website", "price", "cost", "make me", "order", "instagram", "facebook" };
        private static readonly List<string> _autoApprovedUsers = new List<string> { "zapatodefuego", "marine775", "fiskedyret", "barclid", "cweees", "cosmicrave", "refgent" };
        private readonly ILogger _logger;
        private readonly ISubredditService _service;
        private readonly FlairV2 _makerPostFlair;

        public MakerPostCommentHandler(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _makerPostFlair = service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));
        }

        public async Task<bool> Process(BaseController baseController, Func<string, Task> callback)
        {
            var comment = baseController as Comment;
            if (comment == null)
            {
                return false;
            }

            var linkFlairId = comment.Root.Listing.LinkFlairTemplateId;

            // Check if the link flair matches the maker post flait and that the author is not a moderator
            if (linkFlairId != null && linkFlairId.Equals(_makerPostFlair.Id))
            {
                if (comment.Removed || comment.Listing.Approved || CommentAuthorAutoApproved(comment.Author.ToLower()))
                {
                    return false;
                }

                if (_forbiddenPhrases.Any(f => comment.Body.Contains(f, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!DryRun)
                    {
                        comment.Remove();

                        if (callback != null)
                        {
                            await callback($"Removed comment by {comment.Author} on a Maker Post: {_urlRoot}{comment.Permalink}");
                        }

                        _logger.Information($"Removed comment from {comment.Author}: {string.Concat(comment.Body.Take(100))}");
                    }

                    return true;
                }
            }

            return false;
        }

        private bool CommentAuthorAutoApproved(string authorLowerCase)
        {
            return authorLowerCase.Equals(_service.Account.Me.Name) || _autoApprovedUsers.Any(user => authorLowerCase == user);
        }
    }
}
