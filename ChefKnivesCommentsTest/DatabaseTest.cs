using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess;
using ChefKnivesCommentsDatabase;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ChefknivesBot.DataAccess.Tests
{
    internal class TestCommentDatabase : DatabaseService<Comment>
    {
        public TestCommentDatabase(string databaseName)
            : base(GetConnectionString(), databaseName, databaseName) { }

        protected override void UpsertIntoCollection(Thing thing)
        {
            throw new Exception("UpsertIntoCollection was hit");
        }

        public static string GetConnectionString()
        {
            return "mongodb://localhost";
        }
    }

    internal class TestPostDatabase : DatabaseService<Post>
    {
        public TestPostDatabase(string databaseName)
            : base(GetConnectionString(), databaseName, databaseName) { }

        protected override void UpsertIntoCollection(Thing thing)
        {
            throw new Exception("UpsertIntoCollection was hit");
        }

        public static string GetConnectionString()
        {
            return "mongodb://localhost";
        }
    }

    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void CachePreventsUpsertComments()
        {
            var database = new TestCommentDatabase(databaseName: "test");
            var comments = new List<Comment>()
            {
                new Comment()
                {
                    Author = "test",
                    Body = "this is a comment",
                    Id = "1",
                    PostLinkId = "123"
                }
            };

            // first insert should hit upsert, that's fine
            Assert.ThrowsException<Exception>(() => database.Upsert(comments));

            // second insert must be caught by the cache, else we have a serious problem
            database.Upsert(comments);
        }

        [TestMethod]
        public void CachePreventsUpsertPosts()
        {
            var database = new TestPostDatabase(databaseName: "test");
            var posts = new List<Post>()
            {
                new Post()
                {
                    Author = "test",
                    Title = "this is a comment",
                    Id = "1"
                }
            };

            // first insert should hit upsert, that's fine
            Assert.ThrowsException<Exception>(() => database.Upsert(posts));

            // second insert must be caught by the cache, else we have a serious problem
            database.Upsert(posts);
        }
    }
}
