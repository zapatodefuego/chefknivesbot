using ChefKnifeSwapBot.Handlers;
using ChefKnivesBot.Handlers.Comments;
using ChefKnivesBot.Handlers.Mail;
using ChefKnivesBot.Handlers.Posts;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using SubredditBot.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SubredditBot.Cli
{
    public class TestSubredditBotInitializer
    {
        private const string _subredditName = "zapatodefuego";

        public SubredditService Start(ILogger logger, IConfiguration configuration, bool dryRun = false)
        {
            logger.Information("Application started...");

            if (dryRun)
            {
                logger.Warning("This is a DRYRUN! No actions will be taken!");
            }

            var redditClient = new RedditClient(appId: configuration["AppId"], appSecret: configuration["AppSecret"], refreshToken: configuration["RefreshToken"]);
            var service = new SubredditService(logger, configuration, redditClient, subredditName: _subredditName, databaseName: _subredditName);

            // chefknives comments
            service.CommentHandlers.Add(new MakerPostCommentHandler(logger, service, dryRun));
            service.CommentHandlers.Add(new MakerPostReviewCommand(logger, service, dryRun));

            // chefknives posts
            service.PostHandlers.Add(new KnifePicsPostHandler(logger, service, dryRun));
            service.PostHandlers.Add(new MakerPostHandler(logger, service, dryRun));
            service.PostHandlers.Add(new RecommendMePostHandler(logger, service, dryRun));
            service.PostHandlers.Add(new TenToOnePostHandler(logger, service, dryRun));

            // chefknives messages
            //service.MessageHandlers.Add(new MessageHandler(logger, service, dryRun));

            // chefknifeswap posts
            service.PostHandlers.Add(new SwapPostHandler(logger, service, dryRun));

            // ryky comments
            service.CommentHandlers.Add(new RykyPraiseCommandHandler(logger, service, dryRun));

            service.SubscribeToPostFeed();
            service.SubscribeToCommentFeed();
            service.SubscribeToMessageFeed();

            service.RegisterRepeatForCommentAndPostDataPull();

            return service;
        }
    }
}
