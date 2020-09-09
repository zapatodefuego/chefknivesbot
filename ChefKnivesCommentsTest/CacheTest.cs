using ChefKnivesBot.Data;
using ChefKnivesCommentsDatabase;
using ChefKnivesCommentsDatabase.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChefknivesBot.DataAccess.Tests
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void AddWorksAsExpectedBeforeReachingCapacity()
        {
            DatabaseCache<int> cache = new DatabaseCache<int>(10);
            for (int i = 0; i < 10; ++i)
            {
                Assert.IsFalse(cache.Contains(i));
                cache.Add(i);
                Assert.IsTrue(cache.Contains(i));
            }
        }

        [TestMethod]
        public void AddWorksAsExpectedAfterReachingCapacity()
        {
            DatabaseCache<int> cache = new DatabaseCache<int>(5);
            for (int i = 0; i < 10; ++i)
            {
                Assert.IsFalse(cache.Contains(i));
                cache.Add(i);
                Assert.IsTrue(cache.Contains(i));
            }

            Assert.IsFalse(cache.Contains(0));
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
