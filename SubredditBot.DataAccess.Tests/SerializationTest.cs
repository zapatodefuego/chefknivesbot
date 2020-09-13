using ChefKnivesCommentsDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SubredditBot.DataAccess.Serialization;
using System.IO;
using System.Linq;

namespace ChefknivesBot.DataAccess.Tests
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void DoesJsonDeserialize()
        {
            string jsonMessage = File.ReadAllText("HttpSampleResponse.json");
            RedditCommentQueryResponse response = JsonConvert.DeserializeObject<RedditCommentQueryResponse>(jsonMessage);

            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.data.children.Length, "Two comments should be deserialized");

            for (int i = 0; i < 2; ++i)
            {
                Assert.AreEqual($"author{i + 1}", response.data.children[i].data.author);
            }
        }

        [TestMethod]
        public void RedditCommentReaderCanRetrieveMostRecentComment()
        {
            RedditHttpsReader reader = new RedditHttpsReader("chefknives");
            SubredditBot.Data.Comment comment = reader.GetRecentComments(1).FirstOrDefault();

            Assert.IsNotNull(comment);
            Assert.IsNotNull(comment.Author);
            Assert.IsNotNull(comment.Body);
            Assert.IsNotNull(comment.Id);
            Assert.IsNotNull(comment.PostLinkId);
        }

        [TestMethod]
        public void RedditCommentReaderCanRetrieveMostRecent10Comments()
        {
            RedditHttpsReader reader = new RedditHttpsReader("chefknives");
            var comments = reader.GetRecentComments(10);

            Assert.AreEqual(10, comments.Count());

            foreach (SubredditBot.Data.Comment comment in comments)
            {
                Assert.IsNotNull(comment);
                Assert.IsNotNull(comment.Author);
                Assert.IsNotNull(comment.Body);
                Assert.IsNotNull(comment.Id);
                Assert.IsNotNull(comment.PostLinkId);
            }
        }

        [TestMethod]
        public void AllRedditCommentsShouldHaveT3InLinkId()
        {
            RedditHttpsReader reader = new RedditHttpsReader("chefknives");
            var comments = reader.GetRecentComments(100);

            Assert.AreEqual(100, comments.Count());

            foreach (SubredditBot.Data.Comment comment in comments)
            {
                Assert.IsNotNull(comment);
                Assert.IsNotNull(comment.PostLinkId);
                Assert.IsTrue(comment.PostLinkId.Contains("t3_"), $"{comment.PostLinkId} is expected to contain \"t3_\"");
            }
        }
    }
}
