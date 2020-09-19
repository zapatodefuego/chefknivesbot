using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using Comment = Reddit.Controllers.Comment;

namespace ChefKnivesBot.Handlers.Comments
{
    public class MakerPostCommentHandler : HandlerBase, ICommentHandler
    {
        private static List<string> _forbiddenPhrases = new List<string> { "buy", "sell", "website", "price", "cost", "make me", "order", "instagram", "facebook" };
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
                !_service.Subreddit.Moderators.Any(m => m.Name.Equals(comment.Author)))
            {
                // TODO: Check if we replied with the right thing first...
                if (comment.Removed || comment.Listing.Approved || comment.Replies.Any(c => c.Author.Equals(_service.Account.Me.Name)))
                {
                    return false;
                }

                if (_forbiddenPhrases.Any(f => comment.Body.Contains(f, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!DryRun)
                    {
                        comment.Remove();

                        // Send message to modmail about the removal for review
                        _service.Account.Modmail.NewConversation(
                            body: $"Removed comment by {comment.Author}: {comment.Permalink}",
                            subject: $"{nameof(MakerPostCommentHandler)}: comment removal review",
                            srName: _service.Subreddit.Name,
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
