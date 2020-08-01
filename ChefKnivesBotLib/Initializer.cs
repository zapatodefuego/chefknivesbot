using ChefKnivesBotLib.Handlers.Comments;
using ChefKnivesBotLib.Handlers.Posts;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using System;
using System.Linq;

namespace ChefKnivesBotLib
{
    public class Initializer
    {
        private const string _subredditName = "chefknives";
        private const string _modMailConversationId = "fy2tg";

        public static ChefKnivesListener Start(ILogger logger, string settingsFileLocation)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(settingsFileLocation, false, true)
                .Build();

            logger.Information("Application started...");

            var redditClient = new RedditClient(appId: configuration["AppId"], appSecret: configuration["AppSecret"], refreshToken: configuration["RefreshToken"]);
            var subreddit = redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals(_subredditName));
            var account = redditClient.Account;
            var makerPostFlair = subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));

            var listener = new ChefKnivesListener(logger, redditClient, subreddit);

            listener.CommentHandlers.Add(new MakerPostCommentHandler(logger, subreddit, account));
            listener.CommentHandlers.Add(new MakerPostReviewCommand(logger, redditClient, subreddit, account));

            listener.PostHandlers.Add(new MakerPostHandler(logger, makerPostFlair, account));
            listener.PostHandlers.Add(new TenToOnePostHandler(logger, redditClient, subreddit, account));

            listener.SubscribeToPostFeed();
            listener.SubscribeToCommentFeed();

            return listener;
        }
    }
}
