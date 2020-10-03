using SubredditBot.Lib;
using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using ChefKnivesBot;
using SubredditBot.DataAccess;
using ChefKnivesBot.Handlers.Comments;
using ChefKnivesBot.Handlers.Posts;
using ChefKnifeSwapBot.Handlers;
using ChefKnivesBot.Wiki;
using System.Threading;

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

            var ChefKnivesSettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["ChefKnivesSettingsFile"]);
            var compoundConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            if (!string.IsNullOrEmpty(ChefKnivesSettingsFile))
            {
                compoundConfiguration.AddJsonFile(ChefKnivesSettingsFile, true, false);
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
            else if (args.Any(a => a.Equals("--testchefknivesbot")))
            {
                Console.WriteLine("Running ChefKnivesBot on r/zapatodefuego.");
                Console.WriteLine("Press any key to exit...");

                var service = new TestSubredditBotInitializer().Start(Log.Logger, _configuration, false);
                InitializeTestServiceFunctions(Log.Logger, service, false);

                Console.ReadKey();
                service.Dispose();
            }
            else if (args.Any(a => a.Equals("--chefkniveswiki")))
            {
                Console.WriteLine("Executing wiki functions");
                var service = new TestSubredditBotInitializer().Start(Log.Logger, _configuration, false);

                var reviewPage = new ReviewPage(Log.Logger, service);
                reviewPage.AddReviewLinkToReviewPage("A", "zapatodefuego", "test entry 0", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("B", "zapatodefuego", "test entry 1", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("C", "zapatodefuego", "test entry 2", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("X", "zapatodefuego", "test entry 3", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("Y", "zapatodefuego", "test entry 4", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("Z", "zapatodefuego", "test entry 5", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");

                reviewPage.AddReviewLinkToReviewPage("A", "zapatodefuego", "test entry 10", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("B", "zapatodefuego", "test entry 11", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("C", "zapatodefuego", "test entry 12", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("X", "zapatodefuego", "test entry 13", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("Y", "zapatodefuego", "test entry 14", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
                reviewPage.AddReviewLinkToReviewPage("Z", "zapatodefuego", "test entry 15", "https://www.reddit.com/r/chefknives/wiki/edit/reviews");
            }
        }

        private static void InitializeTestServiceFunctions(ILogger logger, SubredditService service, bool dryRun)
        {
            // chefknives comments
            service.CommentHandlers.Add(new MakerPostCommentHandler(logger, service, dryRun));

            // chefknives posts
            service.PostHandlers.Add(new KnifePicsPostHandler(logger, service, dryRun));
            service.PostHandlers.Add(new MakerPostHandler(logger, service, dryRun));
            service.PostHandlers.Add(new RecommendMePostHandler(logger, service, dryRun));

            // chefknives messages
            //service.MessageHandlers.Add(new MessageHandler(logger, service, dryRun));

            // chefknifeswap posts
            service.PostHandlers.Add(new SwapPostHandler(logger, service, dryRun));

            // ryky comments
            //service.CommentHandlers.Add(new RykyPraiseCommandHandler(logger, service, dryRun));

            service.SubscribeToPostFeed();
            service.SubscribeToCommentFeed();
            service.SubscribeToMessageFeed();

            service.RegisterRepeatForCommentAndPostDataPull();
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
