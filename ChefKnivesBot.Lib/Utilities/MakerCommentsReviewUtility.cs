using ChefKnivesBot.Lib.Data;
using Reddit;
using Reddit.Inputs.Search;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ChefKnivesBot.Lib.Utilities
{

    public class MakerCommentsReviewUtility
    {
        private static Stopwatch _stopWatch = new Stopwatch();
        private const int _commentQueryCount = 60;

        public static MakerReviewResult Review(ILogger logger, string author, string subreddit, RedditClient redditClient)
        {
            _stopWatch.Reset();
            _stopWatch.Start();

            var potentialUsers = redditClient.SearchUsers(new SearchGetSearchInput(author));
            var user = potentialUsers.SingleOrDefault(u => u.Name.Equals(author));

            if (user == null)
            {
                return new MakerReviewResult { Error = $"Unable to find user {author}" };
            }

            user.GetCommentHistory(_commentQueryCount);
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
            logger.Information($"[{nameof(MakerCommentsReviewUtility)}] Queried for {_commentQueryCount} comments for {author} in [{_stopWatch.ElapsedMilliseconds}] miliseconds");

            return new MakerReviewResult
            {
                OtherComments = otherComments,
                SelfPostComments = selfPostComments
            };
        }
    }
}
