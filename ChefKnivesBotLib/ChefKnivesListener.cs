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

        public ChefKnivesListener(ILogger logger, RedditClient redditClient, Subreddit subreddit)
        {
            _logger = logger;
            _redditClient = redditClient;
            _subreddit = subreddit;
        }

        public List<ICommentHandler> CommentHandlers { get; } = new List<ICommentHandler>();

        public List<IPostHandler> PostHandlers { get; } = new List<IPostHandler>();

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
                    CommentHandlers.ForEach(c => c.Process(comment));
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
                    PostHandlers.ForEach(c => c.Process(post));
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
