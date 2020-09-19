using SubredditBot.Lib;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using ChefKnivesBot;
using SubredditBot.DataAccess;

namespace SubredditBot.Cli
{
    public static class Program
    {
        public static SubredditService ChefKnivesService { get; set; }

        public static bool DryRun { get; private set; }

        private static IConfigurationRoot _configuration;

        public static async Task Main(string[] args)
        {
            var initialConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            var redditSettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["RedditSettingsFile"]);
            var compoundConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            if (!string.IsNullOrEmpty(redditSettingsFile))
            {
                compoundConfiguration.AddJsonFile(redditSettingsFile, true, false);
            }

            _configuration = compoundConfiguration.Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            if (args.Any(a => a.Equals("--seedchefknives")))
            {
                Console.WriteLine("Action: Seed for chefknives. Press any key to continue...");
                Console.ReadLine();
                await SeedFor("chefknives");
            }
            
            if (args.Any(a => a.Equals("--testchefknivesbot")))
            {
                Console.WriteLine("Running ChefKnivesBot on r/zapatodefuego.");
                Console.WriteLine("Press any key to exit...");

                var service = new TestSubredditBotInitializer().Start(Log.Logger, _configuration, false);
            }

            Console.ReadLine();
        }

        private static async Task SeedFor(string subredditName)
        {
            var redditClient = new RedditClient(appId: _configuration["AppId"], appSecret: _configuration["AppSecret"], refreshToken: _configuration["RefreshToken"]);
            var subreddit = redditClient.Subreddit(subredditName);

            var postDatabase = new DatabaseService<Data.Post>(
                _configuration["ConnectionString"],
                databaseName: subredditName,
                collectionName: DatabaseConstants.PostsCollectionName);

            var commentDatabase = new DatabaseService<Data.Comment>(
                _configuration["ConnectionString"],
                databaseName: subredditName,
                collectionName: DatabaseConstants.CommentsCollectionName);

            var postSeedUtility = new PostSeedUtility(subreddit, postDatabase);
            await postSeedUtility.Execute();

            var commentSeedUtility = new CommentSeedUtility(redditClient, postDatabase, commentDatabase);
            await commentSeedUtility.Execute();
        }

    }
}
