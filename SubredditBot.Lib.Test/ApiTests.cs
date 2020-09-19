using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Reddit;
using Reddit.Controllers;
using Serilog;
using System;
using System.Linq;

namespace SubredditBot.Lib.Tests
{
    [TestFixture]
    public class ApiTests
    {
        private RedditClient _redditClient;
        private Subreddit _subreddit;

        [OneTimeSetUp]
        public void Setup()
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

            var configuration = compoundConfiguration.Build();

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
    }
}
