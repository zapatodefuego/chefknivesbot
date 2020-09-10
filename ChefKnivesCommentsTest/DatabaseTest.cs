using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess;
using ChefKnivesCommentsDatabase;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ChefknivesBot.DataAccess.Tests
{
    internal class TestDatabase : DatabaseService<RedditComment>
    {
        public TestDatabase(string subreddit)
            : base(GetConnectionString(), DatabaseConstants.ChefKnivesDatabaseName, DatabaseConstants.ChefKnivesSubredditName) { }

        protected override void UpsertIntoCollection(RedditThing thing)
        {
            throw new Exception("UpsertIntoCollection was hit");
        }

        public static string GetConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            return configuration["ConnectionString"];
        }
    }

    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void CachePreventsUpsertComments()
        {
            var database = new TestDatabase(subreddit: "test");
            var comments = new List<RedditComment>()
            {
                new RedditComment()
                {
                    Author = "test",
                    Body = "this is a comment",
                    Id = "1",
                    PostLinkId = "123"
                }
            };

            // first insert should hit upsert, that's fine
            Assert.ThrowsException<Exception>(() => database.Insert(comments));

            // second insert must be caught by the cache, else we have a serious problem
            database.Insert(comments);
        }

        [TestMethod]
        public void CachePreventsUpsertPosts()
        {
            var database = new TestDatabase(subreddit: "test");
            List<RedditPost> posts = new List<RedditPost>()
            {
                new RedditPost()
                {
                    Author = "test",
                    Title = "this is a comment",
                    Id = "1"
                }
            };

            // first insert should hit upsert, that's fine
            Assert.ThrowsException<Exception>(() => database.Insert(posts));

            // second insert must be caught by the cache, else we have a serious problem
            database.Insert(posts);
        }
    }
}
