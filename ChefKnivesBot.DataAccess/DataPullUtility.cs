using ChefKnivesBot.Data;
using ChefKnivesCommentsDatabase;
using Serilog;

namespace ChefKnivesBot.DataAccess
{
    public static class DataPullUtility
    {
        public static void Execute(ILogger logger, string connectionString, int postCount, int commentCount)
        {
            var redditReader = new RedditHttpsReader(subreddit: Constants.ChefKnivesSubredditName);
            using (var redditPostDatabase = new DatabaseService<RedditComment>(connectionString, databaseName: Constants.ChefKnivesDatabaseName, collectionName: Constants.PostsCollectionName))
            {
                var recentPosts = redditReader.GetRecentPosts(numPosts: postCount);
                redditPostDatabase.Insert(recentPosts);
            }

            using (var redditCommentDatabase = new DatabaseService<RedditComment>(connectionString, databaseName: Constants.ChefKnivesDatabaseName, collectionName: Constants.CommentsCollectionName))
            {
                var recentComments = redditReader.GetRecentComments(numComments: commentCount);
                redditCommentDatabase.Insert(recentComments);
            }
        }
    }
}
