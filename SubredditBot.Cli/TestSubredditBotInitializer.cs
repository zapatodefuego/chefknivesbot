using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using SubredditBot.Lib;

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
            return new SubredditService(logger, configuration, redditClient, subredditName: _subredditName, databaseName: _subredditName);
        }
    }
}
