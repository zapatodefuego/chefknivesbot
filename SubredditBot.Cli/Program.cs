using SubredditBot.Lib;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using ChefKnivesBot;

namespace SubredditBot.Cli
{
    class Program
    {
        private const string _chefKnivesName = "chefknives";
        private const string _chefKnifeSwapName = "chefknifeswap";

        public static SubredditService ChefKnivesService { get; set; }

        public static bool DryRun { get; private set; }

        private static IConfigurationRoot _configuration;

        static async Task Main(string[] args)
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

            SeedFor(_chefKnivesName).RunSynchronously();
            SeedFor(_chefKnifeSwapName).RunSynchronously();

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static async Task SeedFor(string subredditName)
        {
            var redditClient = new RedditClient(appId: _configuration["AppId"], appSecret: _configuration["AppSecret"], refreshToken: _configuration["RefreshToken"]);
            var subreddit = redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals(subredditName));
            var account = redditClient.Account;

            var chefKnivesBotInitializer = new ChefKnivesBotInitializer();
            ChefKnivesService = chefKnivesBotInitializer.Start(Log.Logger, _configuration, DryRun);

            var commentSeedUtility = new CommentSeedUtility(ChefKnivesService);
            await commentSeedUtility.Execute();

            var postSeedUtility = new PostSeedUtility(ChefKnivesService);
            await postSeedUtility.Execute();
        }
    }
}
