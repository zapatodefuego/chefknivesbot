﻿using ChefKnivesBotLib.Handlers;
using ChefKnivesBotLib.Handlers.Comments;
using ChefKnivesBotLib.Handlers.Posts;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using Reddit.Exceptions;
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
        private readonly Account _account;

        public ChefKnivesListener(ILogger logger, RedditClient redditClient, Subreddit subreddit, Account account)
        {
            _logger = logger;
            _redditClient = redditClient;
            _subreddit = subreddit;
            _account = account;
        }

        public List<IControllerHandler> CommentHandlers { get; } = new List<IControllerHandler>();

        public List<IControllerHandler> PostHandlers { get; } = new List<IControllerHandler>();

        public List<IThingHandler> MessageHandlers { get; } = new List<IThingHandler>();

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

        public void SubscribeToMessageFeed()
        {
            _account.Messages.GetMessagesUnread();
            _account.Messages.UnreadUpdated += Messages_UnreadUpdated;
            _account.Messages.MonitorUnread();
        }

        private void Messages_UnreadUpdated(object sender, MessagesUpdateEventArgs e)
        {
            try
            {
                foreach (var message in e.NewMessages)
                {
                    MessageHandlers.ForEach(c => 
                    {
                        Diagnostics.SeenMessages++;
                        if (c.Process(message))
                        {
                            Diagnostics.ProcessedMessages++;
                        }
                    });
                }
            }
            catch (RedditServiceUnavailableException exception)
            {
                Diagnostics.RedditServiceUnavailableExceptionCount++;
                _logger.Error(exception, $"Exception caught in {nameof(Messages_UnreadUpdated)}. Redoing event and continuing...");
                _account.Messages.InboxUpdated -= Messages_UnreadUpdated;
                SubscribeToMessageFeed();
            }
            catch (Exception exception)
            {
                Diagnostics.OtherExceptionCount++;
                _logger.Error(exception, $"Unexpected exception caught in {nameof(Messages_UnreadUpdated)}");
            }
        }

        private void Comments_NewUpdated(object sender, CommentsUpdateEventArgs e)
        {
            try
            {
                foreach (var comment in e.NewComments)
                {
                    CommentHandlers.ForEach(c =>
                    {
                        Diagnostics.SeenComments++;
                        if (c.Process(comment))
                        {
                            Diagnostics.ProcessedComments++;
                        }
                    });
                }
            }
            catch (RedditServiceUnavailableException exception)
            {
                Diagnostics.RedditServiceUnavailableExceptionCount++;
                _logger.Error(exception, $"Exception caught in {nameof(Comments_NewUpdated)}. Redoing event and continuing...");
                _subreddit.Comments.NewUpdated -= Comments_NewUpdated;
                SubscribeToCommentFeed();
            }
            catch (Exception exception)
            {
                Diagnostics.OtherExceptionCount++;
                _logger.Error(exception, $"Unexpected exception caught in {nameof(Comments_NewUpdated)}");
            }
        }

        private void Posts_NewUpdated_OrEdited(object sender, PostsUpdateEventArgs e)
        {
            try
            {
                foreach (var post in e.Added)
                {
                    PostHandlers.ForEach(c =>
                    {
                        Diagnostics.SeenPosts++;
                        if (c.Process(post))
                        {
                            Diagnostics.ProcessedComments++;
                        }
                    });
                }
            }
            catch (RedditServiceUnavailableException exception)
            {
                Diagnostics.RedditServiceUnavailableExceptionCount++;
                _logger.Error(exception, $"Exception caught in {nameof(Posts_NewUpdated_OrEdited)}. Redoing event and continuing...");
                _subreddit.Posts.NewUpdated -= Posts_NewUpdated_OrEdited;
                SubscribeToPostFeed();
            }
            catch (Exception exception)
            {
                Diagnostics.OtherExceptionCount++;
                _logger.Error(exception, $"Unexpected exception caught in {nameof(Posts_NewUpdated_OrEdited)}");
            }
        }
    }
}
