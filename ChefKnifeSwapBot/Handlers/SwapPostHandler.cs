using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChefKnifeSwapBot.Handlers
{
    public class SwapPostHandler : HandlerBase, IPostHandler
    {
        private const string _postUrlFirstPart = "https://www.reddit.com/r/chefknifeswap/comments/";

        private ILogger _logger;
        private readonly SubredditService _service;

        public SwapPostHandler(ILogger logger, SubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
        }

        public bool Process(BaseController controller)
        {
            var post = controller as SelfPost;
            if (post == null)
            {
                return false;
            }

            if (_service.SelfCommentDatabase.ContainsAny(nameof(SelfComment.ParentId), post.Id).Result)
            {
                return false;
            }
            if (!DryRun)
            {
                var postHistory = _service.RedditPostDatabase.GetByFilter(nameof(RedditThing.Author), post.Author).Result;
                var replyMessage = new StringBuilder();
                if (postHistory == null || !postHistory.Any())
                {
                    replyMessage.AppendLine($"u/{post.Author} has not submitted any posts in r/{_service.Subreddit.Name} since I've gained sentience");
                }
                else
                {
                    replyMessage.AppendLine($"Here are some past posts from u/{post.Author}:");
                    postHistory.Take(5).ToList()
                        .ForEach(p => replyMessage.AppendLine($"* [{p.Title}]({_postUrlFirstPart}{p.Id})"));
                }

                var reply = post
                    .Reply(replyMessage.ToString())
                    .Distinguish("yes", true);

                _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                _service.RedditPostDatabase.Upsert(post.ToPost());

                return true;
            }

            return false;
        }
    }
}
