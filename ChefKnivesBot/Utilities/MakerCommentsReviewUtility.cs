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

        public static async Task<MakerReviewResult> Review(string author, DatabaseService<Post> postDatabase, DatabaseService<Comment> commentDatabase)
        {
            _stopWatch.Reset();
            _stopWatch.Start();
            var result = new MakerReviewResult();

            var comments = await commentDatabase.GetBy(nameof(RedditThing.Author), author);
            foreach (var comment in comments)
            {
                var post = postDatabase.GetById(ConvertListingIdToPostId(comment.PostLinkId));
                if (post == null)
                {
                    continue;
                }

                if (post.Author.Equals(author))
                {
                    result.SelfPostComments++;
                }
                else
                {
                    result.OtherComments++;
                }
            }

            _stopWatch.Stop();
            result.ReviewTime = _stopWatch.ElapsedMilliseconds;

            return result;
        }

        [Obsolete]
        public static MakerReviewResult ReviewViaApi(ILogger logger, string author, string subreddit, RedditClient redditClient, int limit = 60)
        {
            _stopWatch.Reset();
            _stopWatch.Start();

            var user = redditClient.User(author).About();
            user.GetCommentHistory(limit: limit);
            var commentHistoryForChefknives =
                user
                    .CommentHistory
                    .Where(c => c.Subreddit.Equals(subreddit))
                    .ToList();

            var selfPostComments = 0;
            var otherComments = 0;
            foreach (var comment in commentHistoryForChefknives)
            {
                if (comment.Root.Author.Equals(user.Name))
                {
                    selfPostComments++;
                }
                else
                {
                    otherComments++;
                }
            }

            _stopWatch.Stop();

            Diagnostics.AddReviewTime(_stopWatch.ElapsedMilliseconds);
            logger.Information($"[{nameof(MakerCommentsReviewUtility)}] Queried for {limit} comments for {author} in [{_stopWatch.ElapsedMilliseconds}] miliseconds");

            return new MakerReviewResult
            {
                OtherComments = otherComments,
                SelfPostComments = selfPostComments,
                ReviewTime = _stopWatch.ElapsedMilliseconds
            };
        }

        private static string ConvertListingIdToPostId(string listingId)
        {
            return listingId.Replace($"t3_", "");
        }
    }
}
