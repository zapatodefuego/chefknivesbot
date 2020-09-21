using ChefKnivesCommentsDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubredditBot.Data;
using SubredditBot.DataAccess;
using System;
using System.Collections.Generic;

namespace ChefknivesBot.DataAccess.Tests
{
    internal class TestCommentDatabase : DatabaseService<Comment>
    {
        public TestCommentDatabase(string databaseName)
            : base(GetConnectionString(), databaseName, databaseName) { }

        protected override void UpsertIntoCollection(RedditThing thing)
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

        protected override void UpsertIntoCollection(RedditThing thing)
        {
            throw new Exception("UpsertIntoCollection was hit");
        }

        public static string GetConnectionString()
        {
            return "mongodb://localhost";
        }
    }

    internal class TestPostDatabaseWithNoUpsert : DatabaseService<Post>
    {
        public TestPostDatabaseWithNoUpsert(string databaseName)
            : base(GetConnectionString(), databaseName, databaseName) { }

        protected override void UpsertIntoCollection(RedditThing thing)
        {
            return;
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

        [TestMethod]
        public void ObjectUpdateWithNewValuesReturnsUpdatedObject()
        {
            var database = new TestPostDatabaseWithNoUpsert(databaseName: "test");
            var post1 = new Post
            {
                Id = "Id",
                Fullname = "Fullname"
            };

            var post2 = new Post
            {
                Id = "Id",
                Fullname = "Fullname",
                Flair = "Flair"
            };

            var result1 = database.Upsert(post1);
            var result2 = database.Upsert(post2);
            var result3 = database.Upsert(post2);

            Assert.AreEqual(null, result1);
            Assert.AreEqual(post2, result2);
            Assert.AreEqual(null, result3);
        }
    }
}
