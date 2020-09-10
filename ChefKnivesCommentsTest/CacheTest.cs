using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess.Utility;
using ChefKnivesCommentsDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChefknivesBot.DataAccess.Tests
{
    [TestClass]
    public class CacheTest
    {
        private class IntThing : RedditThing
        {
            public int Index { get; set; }
        }

        [TestMethod]
        public void AddWorksAsExpectedBeforeReachingCapacity()
        {
            DatabaseCache<RedditThing> cache = new DatabaseCache<RedditThing>(10);
            for (int i = 0; i < 10; ++i)
            {
                var item = new IntThing { Index = i };
                Assert.IsFalse(cache.Contains(item));
                cache.Add(item);
                Assert.IsTrue(cache.Contains(item));
            }
        }

        [TestMethod]
        public void AddWorksAsExpectedAfterReachingCapacity()
        {
            DatabaseCache<RedditThing> cache = new DatabaseCache<RedditThing>(5);
            for (int i = 0; i < 10; ++i)
            {
                var item = new IntThing { Index = i };
                Assert.IsFalse(cache.Contains(item));
                cache.Add(item);
                Assert.IsTrue(cache.Contains(item));
            }

            var newItem = new IntThing { Index = int.MaxValue };
            Assert.IsFalse(cache.Contains(newItem));
        }

        [TestMethod]
        public void DatabaseCacheWorksWithDatabaseObjects()
        {
            DatabaseCache<RedditComment> cache = new DatabaseCache<RedditComment>(5);
            for (int i = 0; i < 10; ++i)
            {
                RedditComment comment = new RedditComment() { Body = $"info: {i}" };
                Assert.IsFalse(cache.Contains(comment));
                cache.Add(comment);
                Assert.IsTrue(cache.Contains(comment));
            }

            Assert.IsFalse(cache.Contains(new RedditComment() { Body = $"info: 0" }));
        }

        [TestMethod]
        public void ObjectEqualityOnDatabaseObjectsWorksAsExpected()
        {
            using (DatabaseCache<RedditComment> cache = new DatabaseCache<RedditComment>(5))
            {
                for (int i = 0; i < 5; ++i)
                {
                    RedditComment comment = new RedditComment() { Body = $"info: {i}" };
                    Assert.IsFalse(cache.Contains(comment));
                    cache.Add(comment);
                    Assert.IsTrue(cache.Contains(comment));
                }

                Assert.IsTrue(cache.Contains(new RedditComment() { Body = $"info: 0" }));
            }

            using (DatabaseCache<RedditPost> postCache = new DatabaseCache<RedditPost>(15))
            {
                for (int i = 0; i < 20; ++i)
                {
                    RedditPost post = new RedditPost() { Title = $"info: {i}" };
                    Assert.IsFalse(postCache.Contains(post));
                    postCache.Add(post);
                    Assert.IsTrue(postCache.Contains(post));
                }

                Assert.IsTrue(postCache.Contains(new RedditPost() { Title = $"info: 10" }));
                Assert.IsFalse(postCache.Contains(new RedditPost() { Title = $"info: 0" }));
            }
        }
    }
}
