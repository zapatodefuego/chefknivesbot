using Reddit;
using Reddit.Controllers;
using SubredditBot.DataAccess;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Threading.Tasks;

namespace SubredditBot.Cli
{
    public class CommentSeedUtility
    {
        private readonly RedditClient _redditClient;
        private readonly IDatabaseService<Data.Post> _postDatabase;
        private readonly IDatabaseService<Data.Comment> _commentDatabase;

        public CommentSeedUtility(RedditClient redditClient, IDatabaseService<Data.Post> postDatabase, IDatabaseService<Data.Comment> commentDatabase)
        {
            _redditClient = redditClient;
            _postDatabase = postDatabase;
            _commentDatabase = commentDatabase;
        }

        public async Task Execute()
        {
            var commentCount = 0;
            var postCount = 0;

            Console.WriteLine("Pulling comments for known posts...");

            var posts = await _postDatabase.GetAll();

            foreach (var post in posts)
            {
                postCount++;

                // Because we seed with one invalid post object
                if (string.IsNullOrEmpty(post.Kind))
                {
                    continue;
                }

                var postController = _redditClient.Post($"t3_{post.Id}");
                var comments = postController.Comments.GetComments();
                foreach (var comment in comments)
                {
                    commentCount++;
                    var redditComment = comment.ToComment();
                    _commentDatabase.Upsert(redditComment);
                }

                Console.WriteLine($"Post {post.Id} had {comments.Count} comments");
            }

            Console.WriteLine($"Pulled {commentCount} comments from {postCount} posts");
        }
    }
}
