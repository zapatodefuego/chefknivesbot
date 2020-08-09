﻿using ChefKnivesBotLib.Data;
using ChefKnivesBotLib.Utilities;
using Reddit;
using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using System;
using System.Linq;
using Account = Reddit.Controllers.Account;
using Post = Reddit.Controllers.Post;
using Subreddit = Reddit.Controllers.Subreddit;

namespace ChefKnivesBotLib.Handlers.Posts
{
    public class TenToOnePostHandler : HandlerBase, IControllerHandler
    {
        private readonly ILogger _logger;
        private readonly RedditClient _redditClient;
        private readonly Subreddit _subreddit;
        private readonly Account _account;
        private readonly FlairV2 _makerPostFlair;

        public TenToOnePostHandler(ILogger logger, RedditClient redditClient, Subreddit subreddit, Account account, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _redditClient = redditClient;
            _subreddit = subreddit;
            _account = account;
            _makerPostFlair = _subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));
        }

        public bool Process(BaseController baseController)
        {
            var post = baseController as Post;
            if (post == null)
            {
                return false;
            }

            var linkFlairId = post.Listing.LinkFlairTemplateId;

            // Checkc that the tile contains [maker post] or that the link flair matches the maker post flair
            if (post.Title.Contains("[maker post]", StringComparison.OrdinalIgnoreCase)
                    || (linkFlairId != null && linkFlairId.Equals(_makerPostFlair.Id))
               )
            {
                var result = MakerCommentsReviewUtility.Review(_logger, post.Author, _subreddit.Name, _redditClient);

                if (result.OtherComments < 2)
                {
                    SendNeverContributedWarningMessage(post);
                }
                else if (result.OtherComments < (result.SelfPostComments * 0.75))
                {
                    SendTenToOneWarningMessage(post, result);
                }

                return true;
            }

            return false;
        }

        private void SendNeverContributedWarningMessage(Post post)
        {
            if (!post.Comments.New.Any(c => c.Author.Equals(_account.Me.Name) && c.Body.StartsWith("It looks like you haven't")))
            {
                if (!DryRun)
                {
                    post
                        .Reply(
                            $"It looks like you haven't recently contributed to this community. Please sufficiently interact with r/{_subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes", false);

                    post.Remove();
                }

                _logger.Information($"Commented with SendNeverContributedWarningMessage on post by {post.Author}");
            }
        }

        private void SendTenToOneWarningMessage(Post post, MakerReviewResult result)
        {
            if (!post.Comments.New.Any(c => c.Author.Equals(_account.Me.Name) && c.Body.StartsWith("Of your recent comments in")))
            {
                if (!DryRun)
                {
                    post
                        .Reply(
                            $"Of your recent comments in {_subreddit.Name}, {result.OtherComments} occured outside of your own posts while {result.SelfPostComments} were made on posts your authored. \n\n " +
                            $"Please sufficiently interact with r/{_subreddit.Name} outside of your own posts before submitting a [Maker Post].")
                        .Distinguish("yes", false);

                    post.Remove();
                }

                _logger.Information($"Commented with SendTenToOneWarningMessage on post by {post.Author}. ");
            }
        }
    }
}
