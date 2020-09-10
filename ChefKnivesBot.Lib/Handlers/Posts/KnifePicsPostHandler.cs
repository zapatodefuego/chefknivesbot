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
using ChefKnivesBot.Lib.DataExtensions;
using ChefKnivesBot.Data;

namespace ChefKnivesBot.Lib.Handlers.Posts
{
    public class KnifePicsPostHandler : HandlerBase, IControllerHandler
    {
        private ILogger _logger;
        private readonly ChefKnivesService _service;
        private readonly FlairV2 _knifePicsFlair;
        private readonly Rule _rulefive;

        public KnifePicsPostHandler(ILogger logger, ChefKnivesService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _knifePicsFlair = service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Knife Pics"));
            _rulefive = service.Subreddit.GetRules().Rules.First(r => r.ShortName.Equals("#5 - Descriptive Content"));
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
                if (!_service.SelfCommentDatabase.GetBy(nameof(SelfComment.ParentId), post.Id).Result.Any())
                {
                    if (!DryRun)
                    {
                        var replyComment = post
                            .Reply(
                                $"Please ensure you fulfill Rule #5 by posting a top level comment with a description within 15 minutes. Any post not in compliance will be removed. See Rule #5 below for more information: \n\n" +
                                "---\n\n" +
                                $"{_rulefive.Description}")
                            .Distinguish("yes", true);

                        _service.SelfCommentDatabase.Upsert(replyComment.ToSelfComment(post.Id, RedditThingType.Post));
                        ScheduleDelayedCheck(post, replyComment);
                    }

                    _logger.Information($"[{nameof(KnifePicsPostHandler)}]: Commented with rule five warning on post by {post.Author}");
                }

                return true;
            }

            return false;
        }

        private void ScheduleDelayedCheck(Post post, Comment replyComment)
        {
            // 900000 ms is 15 minutes
            var timer = new Timer { Interval = 900000 };
            timer.Elapsed += (object sender, ElapsedEventArgs e) => 
            {
                timer.Stop();
                timer.Dispose();

                var comments = post.Comments.GetComments();
                if (!comments.Any(c => c.Depth == 0 && c.Author.Equals(post.Author)))
                {
                    post.Remove();
                    _logger.Information($"[{nameof(KnifePicsPostHandler)}]: Removed a post by {post.Author} since they did not post a top level comment within the allowed time.");
                }

                replyComment.Delete();

                // Update the comment in the database. There has to be a more elegant way to do this...
                var databaseComment = replyComment.ToSelfComment(post.Id, RedditThingType.Post);
                databaseComment.IsDeleted = true;
                _service.SelfCommentDatabase.Upsert(databaseComment);
            };

            timer.Enabled = true;
            timer.Start();
        }
    }
}
