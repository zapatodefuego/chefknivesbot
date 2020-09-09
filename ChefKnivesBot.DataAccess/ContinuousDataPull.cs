using ChefKnivesBot.Data;
using ChefKnivesCommentsDatabase;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ChefKnivesBot.DataAccess
{
    public class ContinuousDataPull
    {
        /// <summary>
        /// How long to wait between cycles of retrieving posts/comments
        /// </summary>
        //private static readonly TimeSpan cycleLength = TimeSpan.FromMinutes(30);

        public void ExecuteForever(ILogger logger, IConfiguration configuration, TimeSpan cycleLength)
        {
            var redditReader = new RedditHttpsReader(subreddit: "chefknives");
            using (var redditDatabase = new RedditContentService(configuration, subreddit: "chefknives"))
            {
                while (true)
                {
                    var recentPosts = redditReader.GetRecentPosts(numPosts: 30);
                    redditDatabase.InsertPosts(recentPosts);
                    logger.Information($"I added recent posts");

                    var recentComments = redditReader.GetRecentComments(numComments: 100);
                    redditDatabase.InsertComments(recentComments);
                    logger.Information($"I added recent comments");
                    Thread.Sleep(cycleLength);
                }
            }
        }
    }
}
