﻿using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Comment = Reddit.Controllers.Comment;
using Post = Reddit.Controllers.Post;

namespace CuttingBoardsBot.Handlers
{
    public class BoardPicsPostHandler : HandlerBase, IPostHandler
    {
        private ILogger _logger;
        private readonly ISubredditService _service;
        private readonly FlairV2 _knifePicsFlair;
        private readonly Rule _rulefive;

        public BoardPicsPostHandler(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _knifePicsFlair = service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Board Pics"));
            _rulefive = service.Subreddit.GetRules().Rules.First(r => r.ShortName.Equals("#5 - Descriptive content"));
        }

        public async Task<bool> Process(BaseController baseController, Func<string, Task> callback = null)
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
                var existing = await _service.SelfCommentDatabase.GetAny(nameof(SelfComment.ParentId), post.Id);
                if (existing != null)
                {
                    return false;
                }

                if (!DryRun)
                {
                    var replyComment = post
                        .Reply(
                            $"Please ensure you fulfill Rule #5 by posting a top level comment with a description. Any post not in compliance may be removed. See Rule #5 below for more information: \n\n" +
                            "---\n\n" +
                            $"{_rulefive.Description}\n\n")
                        .Distinguish("yes", true);

                    _service.SelfCommentDatabase.Upsert(replyComment.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));
                }

                _logger.Information($"[{nameof(BoardPicsPostHandler)}]: Commented with rule five warning on post by {post.Author}");

                return true;
            }

            return false;
        }
    }
}
