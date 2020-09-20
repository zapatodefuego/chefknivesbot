using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Reddit;
using Reddit.Controllers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubredditBot.Lib.Tests
{
    [TestFixture]
    public class TestSubredditServiceBuilder
    {
        private ILogger _logger;
        private IConfiguration _configuration;
        private RedditClient _redditClient;
        private string _subredditName;
        private string _databaseName;

        public TestSubredditServiceBuilder(string subredditName)
        {
            _subredditName = subredditName;
            _databaseName = subredditName;
        }

        public TestSubredditServiceBuilder WithDebugSink()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.SerilogDebugSink()
                .CreateLogger();

            return this;
        }

        // Note: Requires a localhost database...
        public TestSubredditServiceBuilder WithDebugConfiguration()
        {
            var data = new Dictionary<string, string>
            {
                { "ConnectionString", "mongodb://localhost" },
                { "AppId", "" },
                { "AppSecret", "" },
                { "RefreshToken", "" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();

            return this;
        }

        public TestSubredditServiceBuilder WithMockedRedditClient()
        {
            return this;
        }

        public ISubredditService Build()
        {
            _redditClient = new RedditClient(appId: _configuration["AppId"], appSecret: _configuration["AppSecret"], refreshToken: _configuration["RefreshToken"]);
            return new SubredditService(_logger, _configuration, _redditClient, _subredditName, _databaseName);
        }
    }
}
