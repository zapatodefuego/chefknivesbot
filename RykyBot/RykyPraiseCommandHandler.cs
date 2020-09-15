using Reddit.Controllers;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Comment = Reddit.Controllers.Comment;

namespace ChefKnivesBot.Handlers.Comments
{
    public class RykyPraiseCommandHandler : HandlerBase, ICommentHandler
    {
        private static List<string> _rykyPhrases = new List<string> { "ryky", "burrfection" };
        private ILogger _logger;
        private readonly SubredditService _service;

        public RykyPraiseCommandHandler(ILogger logger, SubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
        }

        public bool Process(BaseController baseController)
        {
            var comment = baseController as Comment;
            if (comment == null)
            {
                return false;
            }

            if (_service.SelfCommentDatabase.GetBy(nameof(SelfComment.ParentId), comment.Id).Result.Any())
            {
                _logger.Information($"[{nameof(RykyPraiseCommandHandler)}]: Comment {comment.Id} has already been replied to");
                return false;
            }

            if (_rykyPhrases.Any(p => comment.Body.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                if (!DryRun)
                {
                    Comment reply;
                    if (comment.Author.Equals("BarashkaZ", StringComparison.OrdinalIgnoreCase))
                    {
                        reply = comment.Reply("Our Ryky who art on YouTube hallowed be thy brick Thy combo stone come, thy will be done Give us this day our daily brick And forgive us our dull knives as we forgive those who have dulled them Lead us not into actual good knives But deliver us a sponsored recommendation, amen");
                    }
                    else
                    {
                        reply = comment.Reply("Praise be!");
                    }

                    _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(comment.ParentId, RedditThingType.Comment));
                }
            }

            return false;
        }
    }
}
