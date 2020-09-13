using ChefKnivesCommentsDatabase;
using Microsoft.Extensions.Configuration;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using Reddit.Exceptions;
using Serilog;
using SubredditBot.Data;
using SubredditBot.DataAccess;
using SubredditBot.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SubredditBot.Lib
{
    public class SubredditService : IDisposable
    {
        private const int _commentAndPostPullIntervalMinutes = 10;

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private CancellationTokenSource _cancellationToken;

        public SubredditService(ILogger logger, IConfiguration configuration, RedditClient redditClient, string subredditName)
        {
            _logger = logger;
            _configuration = configuration;
            RedditClient = redditClient;

            Subreddit = redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals(subredditName));
            Account = redditClient.Account;

            RedditPostDatabase = new DatabaseService<SubredditBot.Data.Post>(
                configuration["ConnectionString"],
                databaseName: Subreddit.Name,
                collectionName: DatabaseConstants.PostsCollectionName);
            RedditCommentDatabase = new DatabaseService<SubredditBot.Data.Comment>(
                configuration["ConnectionString"],
                databaseName: Subreddit.Name,
                collectionName: DatabaseConstants.CommentsCollectionName);
            SelfCommentDatabase = new DatabaseService<SelfComment>(
                configuration["ConnectionString"],
                databaseName: Subreddit.Name,
                collectionName: DatabaseConstants.SelfCommentsCollectionName);
        }

        public RedditClient RedditClient { get; }

        public Subreddit Subreddit { get; }

        public Account Account { get; }

        public DatabaseService<SubredditBot.Data.Post> RedditPostDatabase { get; }

        public DatabaseService<SubredditBot.Data.Comment> RedditCommentDatabase { get; }

        public DatabaseService<SelfComment> SelfCommentDatabase { get; }

        public void Dispose()
        {
            UnsubscribeAllEvents();
            CommentHandlers = null;
            PostHandlers = null;
            MessageHandlers = null;

            _cancellationToken.Cancel();
        }

        public List<ICommentHandler> CommentHandlers { get; private set; } = new List<ICommentHandler>();

        public List<IPostHandler> PostHandlers { get; private set; } = new List<IPostHandler>();

        public List<IMessageHandler> MessageHandlers { get; private set; } = new List<IMessageHandler>();

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

            Repeater.Repeat(() => PullCommentsAndPosts(), _commentAndPostPullIntervalMinutes * 60, _cancellationToken.Token);
        }

        public string ReviewUser(string username)
        {
            var reviewTask = MakerCommentsReviewUtility.Review(username, RedditPostDatabase, RedditCommentDatabase);
            var result = reviewTask.GetAwaiter().GetResult();

            return $"SelfPostComments: {result.SelfPostComments}, OtherComments: {result.OtherComments}, ReviewTime: {result.ReviewTime} (ms), Error: {result.Error}";
        }

        public void PullCommentsAndPosts(int postCount = 100, int commentCount = 100)
        {
            _logger.Information($"Pulling {postCount} posts and {commentCount} comments. Interval: {_commentAndPostPullIntervalMinutes} minutes");

            var redditReader = new RedditHttpsReader(subreddit: Subreddit.Name);

            var recentPosts = redditReader.GetRecentPosts(numPosts: postCount);
            RedditPostDatabase.Upsert(recentPosts);

            var recentComments = redditReader.GetRecentComments(numComments: commentCount);
            RedditCommentDatabase.Upsert(recentComments);

            _logger.Information($"Fineshed pulling posts and comments.");
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
            catch (RedditGatewayTimeoutException exception)
            {
                Diagnostics.RedditGatewayTimeoutException++;
                _logger.Error(exception, $"Exception caught in {nameof(Messages_UnreadUpdated)}. Redoing event and continuing...");
                Account.Messages.InboxUpdated -= Messages_UnreadUpdated;
                SubscribeToMessageFeed();
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
            catch (RedditGatewayTimeoutException exception)
            {
                Diagnostics.RedditGatewayTimeoutException++;
                _logger.Error(exception, $"Exception caught in {nameof(Comments_NewUpdated)}. Redoing event and continuing...");
                Subreddit.Comments.NewUpdated -= Comments_NewUpdated;
                SubscribeToCommentFeed();
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
                            Diagnostics.ProcessedPosts++;
                        }
                    });
                }
            }
            catch (RedditGatewayTimeoutException exception)
            {
                Diagnostics.RedditGatewayTimeoutException++;
                _logger.Error(exception, $"Exception caught in {nameof(Posts_NewUpdated_OrEdited)}. Redoing event and continuing...");
                Subreddit.Posts.NewUpdated -= Posts_NewUpdated_OrEdited;
                SubscribeToPostFeed();
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
