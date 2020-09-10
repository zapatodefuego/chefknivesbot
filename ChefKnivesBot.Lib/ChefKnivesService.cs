using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess;
using ChefKnivesBot.Lib.Handlers;
using ChefKnivesBot.Lib.Utilities;
using ChefKnivesCommentsDatabase;
using Microsoft.Extensions.Configuration;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using Reddit.Exceptions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChefKnivesBot.Lib
{
    public class ChefKnivesService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private CancellationTokenSource _cancellationToken;

        public ChefKnivesService(
            ILogger logger,
            IConfiguration configuration,
            RedditClient redditClient,
            Subreddit subreddit,
            Account account)
        {
            _logger = logger;
            _configuration = configuration;
            RedditClient = redditClient;
            Subreddit = subreddit;
            Account = account;

            RedditPostDatabase = new DatabaseService<RedditPost>(
                configuration["ConnectionString"],
                databaseName: Subreddit.Name,
                collectionName: DatabaseConstants.PostsCollectionName);
            RedditCommentDatabase = new DatabaseService<RedditComment>(
                configuration["ConnectionString"],
                databaseName: Subreddit.Name,
                collectionName: DatabaseConstants.CommentsCollectionName);
            SelfCommentDatabase = new DatabaseService<RedditComment>(
                configuration["ConnectionString"],
                databaseName: Subreddit.Name,
                collectionName: DatabaseConstants.SelfCommentsCollectionName);
        }

        public RedditClient RedditClient { get; }

        public Subreddit Subreddit { get; }

        public Account Account { get; }

        public DatabaseService<RedditPost> RedditPostDatabase { get; }

        public DatabaseService<RedditComment> RedditCommentDatabase { get; }

        public DatabaseService<RedditComment> SelfCommentDatabase { get; }

        public void Dispose()
        {
            UnsubscribeAllEvents();
            CommentHandlers = null;
            PostHandlers = null;
            MessageHandlers = null;

            _cancellationToken.Cancel();
        }

        public List<IControllerHandler> CommentHandlers { get; private set; } = new List<IControllerHandler>();

        public List<IControllerHandler> PostHandlers { get; private set; } = new List<IControllerHandler>();

        public List<IThingHandler> MessageHandlers { get; private set; } = new List<IThingHandler>();

        public bool IsSubredditModerator(string username)
        {
            if (Subreddit == null)
            {
                return false;
            }

            return Subreddit.Moderators.Any(m => m.Name.Equals(username));
        }

        public void SubscribeToPostFeed()
        {
            Subreddit.Posts.GetNew();
            Subreddit.Posts.NewUpdated += Posts_NewUpdated_OrEdited;
            Subreddit.Posts.MonitorNew();
        }

        public void SubscribeToCommentFeed()
        {
            Subreddit.Comments.GetNew();
            Subreddit.Comments.NewUpdated += Comments_NewUpdated;
            Subreddit.Comments.MonitorNew();
        }

        public void SubscribeToMessageFeed()
        {
            Account.Messages.GetMessagesUnread();
            Account.Messages.UnreadUpdated += Messages_UnreadUpdated;
            Account.Messages.MonitorUnread();
        }

        public void UnsubscribeAllEvents()
        {
            Account.Messages.UnreadUpdated -= Messages_UnreadUpdated;
            Subreddit.Comments.NewUpdated -= Comments_NewUpdated;
            Subreddit.Posts.NewUpdated -= Posts_NewUpdated_OrEdited;
        }

        public void RegisterRepeatForCommentAndPostDataPull()
        {
            _cancellationToken = new CancellationTokenSource();

            Repeater.Repeat(() => PullCommentsAndPosts(), 600, _cancellationToken.Token);
        }

        public string ReviewUser(string username)
        {
            var result = MakerCommentsReviewUtility.Review(username, RedditPostDatabase, RedditCommentDatabase);
            return $"SelfPostComments: {result.SelfPostComments}, OtherComments: {result.OtherComments}, ReviewTime: {result.ReviewTime} (ms), Error: {result.Error}";
        }

        public void PullCommentsAndPosts(int postCount = 30, int commentCount = 100)
        {
            var redditReader = new RedditHttpsReader(subreddit: Subreddit.Name);

            var recentPosts = redditReader.GetRecentPosts(numPosts: postCount);
            RedditPostDatabase.Insert(recentPosts);

            var recentComments = redditReader.GetRecentComments(numComments: commentCount);
            RedditCommentDatabase.Insert(recentComments);
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
                Account.Messages.InboxUpdated -= Messages_UnreadUpdated;
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
                Subreddit.Comments.NewUpdated -= Comments_NewUpdated;
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
                Subreddit.Posts.NewUpdated -= Posts_NewUpdated_OrEdited;
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
