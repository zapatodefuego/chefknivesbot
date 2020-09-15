using ChefKnivesBot.Handlers.Comments;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using SubredditBot.Lib;

namespace RykyBot
{
    public class RykyBotInitializer
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
            var service = new SubredditService(logger, configuration, redditClient, subredditName: _subredditName, databaseName: "rykybot");

            service.CommentHandlers.Add(new RykyPraiseCommandHandler(logger, service, dryRun));
            service.SubscribeToCommentFeed();

            return service;
        }
    }
}
