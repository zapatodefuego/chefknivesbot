using Reddit;
using Serilog;
using SubredditBot.Data;
using SubredditBot.DataAccess;
using SubredditBot.Lib;
using SubredditBot.Lib.Data;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ChefKnivesBot.Utilities
{

    public class MakerCommentsReviewUtility
    {
        private static Stopwatch _stopWatch = new Stopwatch();

        public static async Task<MakerReviewResult> Review(string author, IDatabaseService<Post> postDatabase, IDatabaseService<Comment> commentDatabase)
        {
            _stopWatch.Reset();
            _stopWatch.Start();

            var posts = (await postDatabase.GetBy(nameof(RedditThing.Author), author)).Where(p => p.Flair != null && p.Flair.Equals("Maker Post"));

            var commentCount = 0;
            var comments = await commentDatabase.GetBy(nameof(RedditThing.Author), author);
            foreach (var comment in comments)
            {
                var post = postDatabase.GetById(ConvertListingIdToPostId(comment.PostLinkId));
                if (post == null)
                {
                    continue;
                }

                if (!post.Author.Equals(author) || !(post.Flair != null && post.Flair.Equals("Maker Post")))
                {
                    commentCount++;
                }
            }

            _stopWatch.Stop();

            return new MakerReviewResult
            {
                MakerPosts = posts.Count(),
                GoodCitizenComments = commentCount,
                ReviewTime = _stopWatch.ElapsedMilliseconds

            };
        }       

        private static string ConvertListingIdToPostId(string listingId)
        {
            return listingId.Replace($"t3_", "");
        }
    }
}
