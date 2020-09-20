using NUnit.Framework;
using SubredditBot.Lib;
using SubredditBot.Lib.Tests;

namespace ChefKnivesBot.Tests
{
    [TestFixture]
    public class KnifePictsPostHandlerTests
    {
        private ISubredditService _subredditService;

        [OneTimeSetUp]
        public void Setup()
        {
            _subredditService = new TestSubredditServiceBuilder("chefknives")
                .WithDebugConfiguration()
                .WithDebugSink()
                .WithMockedRedditClient()
                .Build();
        }
    }
}