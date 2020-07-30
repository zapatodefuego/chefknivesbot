using ChefKnivesBotLib.Handlers;
using ChefKnivesBotLib.Handlers.Comments;
using ChefKnivesBotLib.Handlers.Posts;
using Reddit;
using Reddit.Controllers;
using Reddit.Inputs.LinksAndComments;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChefKnivesBotLib
{
    public class ChefKnivesListener
    {
        private readonly ILogger _logger;
        private readonly RedditClient _redditClient;
        private readonly Subreddit _subreddit;
        private static User _me;
        private static Reddit.Things.FlairV2 _makerPostFlair;
        public readonly List<ICommentHandler> _commentHandlers = new List<ICommentHandler>();
        public readonly List<IPostHandler> _postHandlers = new List<IPostHandler>();

        public ChefKnivesListener(ILogger logger, RedditClient redditClient)
        {
            _logger = logger;
            _redditClient = redditClient;
            _subreddit = _redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals("chefknives"));
            _me = _redditClient.Account.Me;
            _makerPostFlair = _subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));

            _commentHandlers.Add(new MakerPostCommentHandler(_logger, _subreddit, _me));
            _commentHandlers.Add(new MakerPostReviewCommand(_logger, _redditClient, _subreddit, _me));

            _postHandlers.Add(new MakerPostHandler(_logger, _makerPostFlair, _me));
            _postHandlers.Add(new TenToOnePostHandler(_logger, _redditClient, _subreddit, _me));

            SubscribeToPostFeed();
            SubscribeToCommentFeed();
        }

        public void SubscribeToPostFeed()
        {
            _subreddit.Posts.GetNew();
            _subreddit.Posts.NewUpdated += Posts_NewUpdated_OrEdited;
            _subreddit.Posts.MonitorNew();
        }

        public void SubscribeToCommentFeed()
        {
            _subreddit.Comments.GetNew();
            _subreddit.Comments.NewUpdated += Comments_NewUpdated;
            _subreddit.Comments.MonitorNew();
        }

        private void Comments_NewUpdated(object sender, Reddit.Controllers.EventArgs.CommentsUpdateEventArgs e)
        {
            try
            {
                foreach (var comment in e.NewComments)
                {
                    _commentHandlers.ForEach(c => c.Process(comment));
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Comments_NewUpdated)}. Redoing event and continuing...");
                _subreddit.Comments.NewUpdated -= Comments_NewUpdated;
                SubscribeToCommentFeed();
            }
        }

        private void Posts_NewUpdated_OrEdited(object sender, Reddit.Controllers.EventArgs.PostsUpdateEventArgs e)
        {
            try
            {
                foreach (var post in e.Added)
                {
                    _postHandlers.ForEach(c => c.Process(post));
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Posts_NewUpdated_OrEdited)}. Redoing event and continuing...");

                _subreddit.Posts.NewUpdated -= Posts_NewUpdated_OrEdited;
                SubscribeToPostFeed();
            }
        }
    }
}
