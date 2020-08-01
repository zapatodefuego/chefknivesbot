using ChefKnivesBotLib.Utilities;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Reddit;
using Reddit.Controllers;
using Reddit.Inputs.Modmail;
using Serilog;
using System;
using System.Linq;

namespace ChefKnivesBotLib.Tests
{
    [TestFixture]
    public class ApiTests
    {
        private RedditClient _redditClient;
        private Subreddit _subreddit;

        [OneTimeSetUp]
        public void Setup()
        {
            var configuration0 = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var configuration = new ConfigurationBuilder()
              .AddJsonFile(configuration0["RedditSettingsFile"], false, true)
              .Build();

            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            _redditClient = new RedditClient(appId: configuration["AppId"], appSecret: configuration["AppSecret"], refreshToken: configuration["RefreshToken"]);
            _subreddit = _redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals("chefknives"));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _redditClient = null;
            _subreddit = null;
        }

        [Test]
        [TestCase("zapatodefuego")]
        public void TestCanReviewByUsername(string username)
        {
            var result = MakerCommentsReviewUtility.Review(Log.Logger, username, _subreddit.Name, _redditClient);
        }

        [Test]
        public void SetupModMailConversation()
        {
            // var converstation = _redditClient.Account.Modmail.NewConversation(body: "Placeholder for bot replies", subject: "CKBot Message Queue", srName: _subreddit.Name, to: "chefknives");
        }
    }
}
