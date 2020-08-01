using ChefKnivesBotLib.Data;
using Reddit;
using Reddit.Inputs.Search;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ChefKnivesBotLib.Utilities
{

    public class MakerCommentsReviewUtility
    {
        private static Stopwatch _stopWatch = new Stopwatch();
        private const int _commentQueryCount = 60;

        public static MakerReviewResult Review(ILogger logger, string author, string subreddit, RedditClient redditClient)
        {
            var potentialUsers = redditClient.SearchUsers(new SearchGetSearchInput(author));
            var user = potentialUsers.SingleOrDefault(u => u.Name.Equals(author));

            if (user == null)
            {
                return new MakerReviewResult { Error = $"Unable to find user {author}" };
            }

            _stopWatch.Reset();
            _stopWatch.Start();
            user.GetCommentHistory(_commentQueryCount);
            _stopWatch.Stop();

            logger.Information($"[{nameof(MakerCommentsReviewUtility)}] Queried for {_commentQueryCount} comments for {author} in [{_stopWatch.ElapsedMilliseconds}] miliseconds");

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

            return new MakerReviewResult
            {
                OtherComments = otherComments,
                SelfPostComments = selfPostComments
            };
        }
    }
}
