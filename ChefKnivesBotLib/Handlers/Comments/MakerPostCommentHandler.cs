using Reddit.Inputs.LinksAndComments;
using Reddit.Things;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Subreddit = Reddit.Controllers.Subreddit;
using User = Reddit.Controllers.User;

namespace ChefKnivesBot.Handlers.Comments
{
    public class MakerPostCommentHandler : ICommentHandler
    {
        private static List<string> _forbiddenPhrases = new List<string> { "buy", "sell", "website", "price", "cost", "make me", "order", "instagram", "facebook" };
        private readonly ILogger _logger;
        private Subreddit _subreddit;
        private FlairV2 _makerPostFlair;
        private User _me;

        public MakerPostCommentHandler(ILogger logger, Subreddit subreddit, User me)
        {
            _logger = logger;
            _subreddit = subreddit;
            _makerPostFlair = _subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));
        }

        public void Process(Reddit.Controllers.Comment comment)
        {
            var linkFlairId = comment.Root.Listing.LinkFlairTemplateId;

            // Check if the link flair matches the maker post flait and that the author is not a moderator
            if (linkFlairId != null &&
                linkFlairId.Equals(_makerPostFlair.Id) &&
                !_subreddit.Moderators.Any(m => m.Name.Equals(comment.Author)))
            {
                // TODO: Check if we replied with the right thing first...
                if (comment.Removed || comment.Listing.Approved || comment.Replies.Any(c => c.Author.Equals(_me.Name)))
                {
                    return;
                }

                if (_forbiddenPhrases.Any(f => comment.Body.Contains(f, StringComparison.OrdinalIgnoreCase)))
                {
                    comment.Report(new LinksAndCommentsReportInput(reason: "CKBot removed comment"));
                    comment.Remove();

                    _logger.Information($"Removed comment from {comment.Author}: {comment.Body.Substring(0, 100)}");
                }
            }
        }
    }
}
