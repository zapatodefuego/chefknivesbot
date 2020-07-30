using ChefKnivesBotLib.Data;
using Reddit;
using Reddit.Inputs.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChefKnivesBot.Utilities
{

    public class MakerCommentsReviewUtility
    {
        public static MakerReviewResult Review(string author, string subreddit, RedditClient redditClient)
        {
            var potentialUsers = redditClient.SearchUsers(new SearchGetSearchInput(author));
            var user = potentialUsers.Single(u => u.Name.Equals(author));

            user.GetCommentHistory(30);

            var lastThirtySubredditComments =
                user
                    .CommentHistory
                    .Where(c => c.Subreddit.Equals(subreddit))
                    .ToList();

            var selfPostComments = 0;
            var otherComments = 0;
            foreach (var comment in lastThirtySubredditComments)
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
