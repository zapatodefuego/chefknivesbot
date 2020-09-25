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
using System.Threading.Tasks;

namespace SubredditBot.Lib
{
    public class SubredditService : IDisposable, ISubredditService
    {
        private const int _commentAndPostPullIntervalMinutes = 10;

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly Func<string, Task> _callback;
        private CancellationTokenSource _cancellationToken;

        public SubredditService(
            ILogger logger,
            IConfiguration configuration,
            RedditClient redditClient,
            string subredditName,
            string databaseName,
            Func<string, Task> callback = null)
        {
            _logger = logger;
            _configuration = configuration;
            RedditClient = redditClient;
            _callback = callback;
            Subreddit = redditClient.Subreddit(subredditName);
            Account = redditClient.Account;

            RedditPostDatabase = new DatabaseService<SubredditBot.Data.Post>(
                configuration["ConnectionString"],
                databaseName: databaseName,
                collectionName: DatabaseConstants.PostsCollectionName);
            RedditCommentDatabase = new DatabaseService<SubredditBot.Data.Comment>(
                configuration["ConnectionString"],
                databaseName: databaseName,
                collectionName: DatabaseConstants.CommentsCollectionName);
            SelfCommentDatabase = new DatabaseService<SelfComment>(
                configuration["ConnectionString"],
                databaseName: databaseName,
                collectionName: DatabaseConstants.SelfCommentsCollectionName);
        }

        public RedditClient RedditClient { get; }

        public Subreddit Subreddit { get; }

        public Account Account { get; }

        public IDatabaseService<SubredditBot.Data.Post> RedditPostDatabase { get; }

        public IDatabaseService<SubredditBot.Data.Comment> RedditCommentDatabase { get; }

        public IDatabaseService<SelfComment> SelfCommentDatabase { get; }

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

        public void PullCommentsAndPosts(int postCount = 100, int commentCount = 100)
        {
            _logger.Information($"Pulling {postCount} posts and {commentCount} comments. Interval: {_commentAndPostPullIntervalMinutes} minutes");

            var redditReader = new RedditHttpsReader(subreddit: Subreddit.Name);

            var recentPosts = redditReader.GetRecentPosts(numPosts: postCount);
            foreach (var postToUpdate in recentPosts)
            {
                var updatedPost = RedditPostDatabase.Upsert(postToUpdate);
                if (updatedPost != null && postToUpdate.Flair != updatedPost.Flair)
                {
                    PostHandlers.ForEach(c =>
                    {
                        var postController = RedditClient.Post(updatedPost.Fullname).Info();
                        c.Process(postController);
                    });
                }
            }

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
                        if (c.Process(message))
                        {
                        }
                    });
                }
            }
            catch (RedditGatewayTimeoutException exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Messages_UnreadUpdated)}. Redoing event and continuing...");
                Account.Messages.InboxUpdated -= Messages_UnreadUpdated;
                SubscribeToMessageFeed();
            }
            catch (RedditServiceUnavailableException exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Messages_UnreadUpdated)}. Redoing event and continuing...");
                Account.Messages.InboxUpdated -= Messages_UnreadUpdated;
                SubscribeToMessageFeed();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Unexpected exception caught in {nameof(Messages_UnreadUpdated)}");
            }
        }

        private async void Comments_NewUpdated(object sender, CommentsUpdateEventArgs e)
        {
            try
            {
                foreach (var comment in e.NewComments)
                {
                    Parallel.ForEach(CommentHandlers, c => c.Process(comment, _callback));
                }
            }
            catch (RedditGatewayTimeoutException exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Comments_NewUpdated)}. Redoing event and continuing...");
                Subreddit.Comments.NewUpdated -= Comments_NewUpdated;
                SubscribeToCommentFeed();
            }
            catch (RedditServiceUnavailableException exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Comments_NewUpdated)}. Redoing event and continuing...");
                Subreddit.Comments.NewUpdated -= Comments_NewUpdated;
                SubscribeToCommentFeed();
            }
            catch (Exception exception)
            {
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
                        if (c.Process(post))
                        {
                        }
                    });
                }
            }
            catch (RedditGatewayTimeoutException exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Posts_NewUpdated_OrEdited)}. Redoing event and continuing...");
                Subreddit.Posts.NewUpdated -= Posts_NewUpdated_OrEdited;
                SubscribeToPostFeed();
            }
            catch (RedditServiceUnavailableException exception)
            {
                _logger.Error(exception, $"Exception caught in {nameof(Posts_NewUpdated_OrEdited)}. Redoing event and continuing...");
                Subreddit.Posts.NewUpdated -= Posts_NewUpdated_OrEdited;
                SubscribeToPostFeed();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Unexpected exception caught in {nameof(Posts_NewUpdated_OrEdited)}");
            }
        }
    }
}
