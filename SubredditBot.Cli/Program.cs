using ChefKnifeSwapBot.Handlers;
using ChefKnivesBot.Handlers.Comments;
using ChefKnivesBot.Handlers.Posts;
using ChefKnivesBot.Wiki;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Reddit;
using Serilog;
using SubredditBot.Data;
using SubredditBot.DataAccess;
using SubredditBot.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            else if (args.Any(a => a.Equals("--reprocessmongo")))
            {

                var mongoClient = new MongoClient(_configuration["ConnectionString"]);
                var collection = mongoClient.GetDatabase("chefknives").GetCollection<BsonDocument>("comments");
                //var redditComments = collection.Find(Builders<BsonDocument>.Filter.Eq("_t", "RedditComment")).ToList();
                //foreach (var redditComment in redditComments)
                //{
                //    var comment = BsonSerializer.Deserialize<Comment>(redditComment);
                //    var bson = comment.ToBsonDocument();
                //    bson.InsertAt(1, new BsonElement("_t", "Comment"));
                //    collection.ReplaceOne(
                //        filter: new BsonDocument("_id", comment.Id),
                //        options: new ReplaceOptions { IsUpsert = true },
                //        replacement: bson);
                //}

                //var bsonResults = collection.AsQueryable();
                //foreach (var r in bsonResults)
                //{
                //    var s = r.GetValue("Author");
                //    var ss = s.AsString.ToLower();
                //}
            }
            else if (args.Any(a => a.Equals("--wordcloud")))
            {
                var mongoClient = new MongoClient(_configuration["ConnectionString"]);
                var collection = mongoClient.GetDatabase("chefknives").GetCollection<BsonDocument>("posts");
                var makerPosts = collection
                    .Find(Builders<BsonDocument>
                    .Filter.Eq("Flair", "Maker Post"))
                    .ToList()
                    .Select(b => BsonSerializer.Deserialize<Post>(b));

                var cloud = new Dictionary<string, int>();
                foreach (var makerPost in makerPosts)
                {
                    var words = makerPost.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                    words.ForEach(w =>
                    {
                        var word = w.ToLower().Trim();
                        if (cloud.TryGetValue(word, out int value))
                        {
                            cloud[word]++;
                        }
                        else
                        {
                            cloud.Add(word, 1);
                        }
                    });
                }

                var builder = new StringBuilder();
                foreach (var kvp in from entry in cloud orderby entry.Value descending select entry)
                {
                    builder.AppendLine($"{kvp.Key} {kvp.Value}");
                }

                File.WriteAllText("out.txt", builder.ToString());
            }
        }

        private static void InitializeTestServiceFunctions(ILogger logger, SubredditService service, bool dryRun)
        {
            // chefknives comments
            service.CommentHandlers.Add(new MakerPostCommentHandler(logger, service, dryRun));

            // chefknives posts
            service.PostHandlers.Add(new KnifePicsPostHandler(logger, service, dryRun));
            service.PostHandlers.Add(new MakerPostHandler(logger, service, dryRun));

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
