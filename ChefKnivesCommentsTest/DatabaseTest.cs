using ChefKnivesBot.Data;
using ChefKnivesCommentsDatabase;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ChefknivesBot.DataAccess.Tests
{
    internal class TestDatabase : RedditContentService
    {
        public TestDatabase(string subreddit)
            : base(GetTestConfiguration(), subreddit) { }

        protected override void UpsertIntoCollection(RedditComment comment)
        {
            throw new Exception("UpsertIntoCollection was hit");
        }

        protected override void UpsertIntoCollection(RedditPost comment)
        {
            throw new Exception("UpsertIntoCollection was hit");
        }

        public static IConfiguration GetTestConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }
    }

    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void CachePreventsUpsertComments()
        {
            RedditContentService database = new TestDatabase(subreddit: "test");
            List<RedditComment> comments = new List<RedditComment>()
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
            Assert.ThrowsException<Exception>(() => database.InsertComments(comments));

            // second insert must be caught by the cache, else we have a serious problem
            database.InsertComments(comments);
        }

        [TestMethod]
        public void CachePreventsUpsertPosts()
        {
            RedditContentService database = new TestDatabase(subreddit: "test");
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
            Assert.ThrowsException<Exception>(() => database.InsertPosts(posts));

            // second insert must be caught by the cache, else we have a serious problem
            database.InsertPosts(posts);
        }
    }
}
