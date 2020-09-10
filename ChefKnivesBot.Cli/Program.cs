using ChefKnivesBot.Lib;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChefKnivesBot.Cli
{
    class Program
    {
        private const string _subreddit = "chefknives";

        public static ChefKnivesService ChefKnivesService { get; set; }

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

            var redditClient = new RedditClient(appId: _configuration["AppId"], appSecret: _configuration["AppSecret"], refreshToken: _configuration["RefreshToken"]);
            var subreddit = redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals(_subreddit));
            var account = redditClient.Account;
            var makerPostFlair = subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));

            ChefKnivesService = new ChefKnivesService(Log.Logger, _configuration, redditClient, subreddit, account);

            var commentSeedUtility = new CommentSeedUtility(ChefKnivesService);
            await commentSeedUtility.Execute();

            var postSeedUtility = new PostSeedUtility(ChefKnivesService);
            await postSeedUtility.Execute();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
