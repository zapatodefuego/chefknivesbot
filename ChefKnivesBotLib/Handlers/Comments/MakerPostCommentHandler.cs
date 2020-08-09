using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Account = Reddit.Controllers.Account;
using Comment = Reddit.Controllers.Comment;
using Subreddit = Reddit.Controllers.Subreddit;

namespace ChefKnivesBotLib.Handlers.Comments
{
    public class MakerPostCommentHandler : HandlerBase, IControllerHandler
    {
        private static List<string> _forbiddenPhrases = new List<string> { "buy", "sell", "website", "price", "cost", "make me", "order", "instagram", "facebook" };
        private readonly ILogger _logger;
        private readonly Subreddit _subreddit;
        private readonly FlairV2 _makerPostFlair;
        private readonly Account _account;

        public MakerPostCommentHandler(ILogger logger, Subreddit subreddit, Account account, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _subreddit = subreddit;
            _makerPostFlair = _subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));
            _account = account;
        }

        public bool Process(BaseController baseController)
        {
            var comment = baseController as Comment;
            if (comment == null)
            {
                return false;
            }

            var linkFlairId = comment.Root.Listing.LinkFlairTemplateId;

            // Check if the link flair matches the maker post flait and that the author is not a moderator
            if (linkFlairId != null &&
                linkFlairId.Equals(_makerPostFlair.Id) &&
                !_subreddit.Moderators.Any(m => m.Name.Equals(comment.Author)))
            {
                // TODO: Check if we replied with the right thing first...
                if (comment.Removed || comment.Listing.Approved || comment.Replies.Any(c => c.Author.Equals(_account.Me.Name)))
                {
                    return false;
                }

                if (_forbiddenPhrases.Any(f => comment.Body.Contains(f, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!DryRun)
                    {
                        comment.Remove();

                        // Send message to modmail about the removal for review
                        _account.Modmail.NewConversation(
                            body: $"Removed comment by {comment.Author}: {comment.Permalink}",
                            subject: $"{nameof(MakerPostCommentHandler)}: comment removal review",
                            srName: _subreddit.Name,
                            to: "chefknives");

                        _logger.Information($"Removed comment from {comment.Author}: {comment.Body.Substring(0, 100)}");
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
