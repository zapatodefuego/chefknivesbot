using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SubredditBot.Cli
{
    public class PostSeedUtility
    {
        private readonly SubredditService _service;

        public PostSeedUtility(SubredditService service)
        {
            _service = service;
        }

        public async Task Execute()
        {
            var posts = _service.Subreddit.Posts.GetNew(limit: 100);

            var tries = 0;
            var count = 0;
            var oldestPost = posts.First();
            var stopTime = DateTimeOffset.Now.AddYears(-2);
            while ((oldestPost.Listing.CreatedUTC > stopTime || count == 1000) && tries < 20)
            {
                Console.WriteLine("Current date: " + oldestPost.Listing.CreatedUTC);
                foreach (var post in posts)
                {
                    if (post.Listing.CreatedUTC < oldestPost.Listing.CreatedUTC)
                    {
                        oldestPost = post;
                    }

                    _service.RedditPostDatabase.Upsert(post.ToPost());
                    count++;
                }

                posts = _service.Subreddit.Posts.GetNew(limit: 100, after: oldestPost.Listing.Name);
                tries++;
            }

            Console.WriteLine($"Added {count} posts");
        }
    }
}
