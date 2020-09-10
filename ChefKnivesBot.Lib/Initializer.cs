using ChefKnivesBot.DataAccess;
using ChefKnivesBot.Lib.Handlers.Comments;
using ChefKnivesBot.Lib.Handlers.Mail;
using ChefKnivesBot.Lib.Handlers.Posts;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using System;
using System.Linq;

namespace ChefKnivesBot.Lib
{
    public class Initializer
    {
        private const string _subredditName = "chefknives";

        public static ChefKnivesService Start(ILogger logger, IConfiguration configuration, bool dryRun = false)
        {
            logger.Information("Application started...");

            if (dryRun)
            {
                logger.Warning("This is a DRYRUN! No actions will be taken!");
            }

            var redditClient = new RedditClient(appId: configuration["AppId"], appSecret: configuration["AppSecret"], refreshToken: configuration["RefreshToken"]);
            var subreddit = redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals(_subredditName));
            var account = redditClient.Account;
            var makerPostFlair = subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));

            var service = new ChefKnivesService(logger, configuration, redditClient, subreddit, account);

            service.CommentHandlers.Add(new MakerPostCommentHandler(logger, subreddit, account, dryRun));
            service.CommentHandlers.Add(new MakerPostReviewCommand(logger, service, dryRun));

            service.PostHandlers.Add(new MakerPostHandler(logger, makerPostFlair, account, dryRun));
            service.PostHandlers.Add(new TenToOnePostHandler(logger, service, dryRun));
            service.PostHandlers.Add(new KnifePicsPostHandler(logger, service, dryRun));

            service.MessageHandlers.Add(new MessageHandler(logger, redditClient, account, dryRun));

            service.SubscribeToPostFeed();
            service.SubscribeToCommentFeed();
            service.SubscribeToMessageFeed();
            service.RegisterRepeatForCommentAndPostDataPull();

            return service;
        }
    }
}
