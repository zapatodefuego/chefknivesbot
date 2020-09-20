using Reddit.Controllers;
using SubredditBot.DataAccess;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Post = SubredditBot.Data.Post;

namespace SubredditBot.Cli
{
    public class PostSeedUtility
    {
        private readonly Subreddit _subreddit;
        private readonly IDatabaseService<Post> _postDatabase;

        public PostSeedUtility(Subreddit subreddit, IDatabaseService<Post> postDatabase)
        {
            _subreddit = subreddit;
            _postDatabase = postDatabase;
        }

        public async Task Execute()
        {
            var posts = _subreddit.Posts.GetNew(limit: 100);

            var tries = 0;
            var count = 0;
            var oldestPost = posts.First();
            var stopTime = DateTimeOffset.Now.AddMonths(-6);
            while ((oldestPost.Listing.CreatedUTC > stopTime || count == 1000) && tries < 20)
            {
                Console.WriteLine("Current date: " + oldestPost.Listing.CreatedUTC);
                foreach (var post in posts)
                {
                    if (post.Listing.CreatedUTC < oldestPost.Listing.CreatedUTC)
                    {
                        oldestPost = post;
                    }

                    _postDatabase.Upsert(post.ToPost());
                    count++;
                }

                posts = _subreddit.Posts.GetNew(limit: 100, after: oldestPost.Listing.Name);
                tries++;
            }

            Console.WriteLine($"Added {count} posts");
        }
    }
}
