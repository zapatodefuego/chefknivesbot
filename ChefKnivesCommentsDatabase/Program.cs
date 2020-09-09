using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChefKnivesCommentsDatabase
{
    class Program
    {
        /// <summary>
        /// How long to wait between cycles of retrieving posts/comments
        /// </summary>
        private static readonly TimeSpan cycleLength = TimeSpan.FromMinutes(30);

        private static IConfigurationRoot _configuration;

        static void Main()
        {
            var initialConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile(Environment.ExpandEnvironmentVariables(initialConfiguration["SecretsFile"]), true, false)
                .Build();

            RedditHttpsReader redditReader = new RedditHttpsReader(subreddit: "chefknives");
            using (RedditContentDatabase redditDatabase = new RedditContentDatabase(_configuration, subreddit: "chefknives"))
            {
                while (true)
                {
                    IEnumerable<RedditPost> recentPosts = redditReader.GetRecentPosts(numPosts: 30);
                    redditDatabase.EnsurePostsInDatabase(recentPosts);
                    Console.WriteLine($"I added recent posts");

                    IEnumerable<RedditComment> recentComments = redditReader.GetRecentComments(numComments: 100);
                    redditDatabase.EnsureCommentsInDatabase(recentComments);
                    Console.WriteLine($"I added recent comments");
                    Thread.Sleep(cycleLength);
                }
            }
        }
    }
}
