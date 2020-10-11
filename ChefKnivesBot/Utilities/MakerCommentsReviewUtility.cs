using Reddit;
using Serilog;
using SubredditBot.Data;
using SubredditBot.DataAccess;
using SubredditBot.Lib;
using SubredditBot.Lib.Data;
using System;
using System.Collections.Generic;
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

            var result = new MakerReviewResult();
            var posts = await postDatabase.GetByQueryable(nameof(RedditThing.Author), author);
            var makerPosts = posts.Where(p => p.Flair != null && p.Flair.Equals("Maker Post"));
            var comments = await commentDatabase.GetByQueryable(nameof(RedditThing.Author), author);
            var makerComments = new List<Comment>();
            foreach (var comment in comments)
            {
                var post = postDatabase.GetById(ConvertListingIdToPostId(comment.PostLinkId));
                if (post == null)
                {
                    continue;
                }

                if (post.Author.Equals(author) && post.Flair != null && post.Flair.Equals("Maker Post"))
                {
                    makerComments.Add(comment);
                }
            }

            _stopWatch.Stop();

            return new MakerReviewResult
            {
                Posts = posts,
                MakerPosts = makerPosts,
                Comments = comments,
                MakerComments = makerComments,
                ReviewTime = _stopWatch.ElapsedMilliseconds

            };
        }       

        private static string ConvertListingIdToPostId(string listingId)
        {
            return listingId.Replace($"t3_", "");
        }
    }
}
