using Reddit;
using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Subreddit = Reddit.Controllers.Subreddit;
using Post = Reddit.Controllers.Post;
using System.Timers;
using Comment = Reddit.Controllers.Comment;

namespace ChefKnivesBotLib.Handlers.Posts
{
    public class KnifePicsPostHandler : HandlerBase, IControllerHandler
    {
        private ILogger _logger;
        private readonly Subreddit _subreddit;
        private readonly Account _account;
        private readonly FlairV2 _knifePicsFlair;
        private readonly Rule _rulefive;

        public KnifePicsPostHandler(ILogger logger, Subreddit subreddit, Account account, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _subreddit = subreddit;
            _account = account;
            _knifePicsFlair = _subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Knife Pics"));
            _rulefive = _subreddit.GetRules().Rules.First(r => r.ShortName.Equals("#5 - Descriptive Content"));
        }

        public bool Process(BaseController baseController)
        {
            var post = baseController as Post;
            if (post == null)
            {
                return false;
            }

            var linkFlairId = post.Listing.LinkFlairTemplateId;
            if (linkFlairId != null && linkFlairId.Equals(_knifePicsFlair.Id))
            {
                // Check if we already commented on this post
                if (!post.Comments.New.Any(c => c.Author.Equals(_account.Me.Name) && c.Body.StartsWith("Please ensure you fulfill Rule #5")))
                {
                    if (!DryRun)
                    {
                        var replyComment = post
                            .Reply(
                                $"Please ensure you fulfill Rule #5 by posting a top level comment with a description within 15 minutes. Any post not in compliance will be removed. See Rule #5 below for more information: \n\n" +
                                "---\n\n" +
                                $"{_rulefive.Description}")
                            .Distinguish("yes", true);

                        ScheduleDelayedCheck(post, replyComment);
                    }

                    _logger.Information($"Commented with rule five warning on post by {post.Author}");
                }

                return true;
            }

            return false;
        }

        private void ScheduleDelayedCheck(Post post, Comment replyComment)
        {
            var timer = new Timer { Interval = 900000 };
            timer.Elapsed += (object sender, ElapsedEventArgs e) => 
            {
                timer.Stop();
                timer.Dispose();

                var comments = post.Comments.GetComments();
                if (!comments.Any(c => c.Depth == 0 && c.Author.Equals(post.Author)))
                {
                    post.Remove();
                    _logger.Information($"Removed a post by {post.Author} since they did not post a top level comment within the allowed time.");
                }

                replyComment.Remove();
            };
            timer.Enabled = true;
            timer.Start();
        }
    }
}
