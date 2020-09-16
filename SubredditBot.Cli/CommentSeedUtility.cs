using Reddit.Controllers;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Threading.Tasks;

namespace SubredditBot.Cli
{
    public class CommentSeedUtility
    {
        private readonly SubredditService _service;

        public CommentSeedUtility(SubredditService service)
        {
            _service = service;
        }

        public async Task Execute()
        {
            var commentCount = 0;
            var postCount = 0;

            Console.WriteLine("Pulling comments for known posts...");

            var posts = await _service.RedditPostDatabase.GetAll();

            foreach (var post in posts)
            {
                postCount++;

                // Because we seed with one invalid post object
                if (string.IsNullOrEmpty(post.Kind))
                {
                    continue;
                }

                var postController = _service.RedditClient.Post($"t3_{post.Id}");
                var comments = postController.Comments.GetComments();
                foreach (var comment in comments)
                {
                    commentCount++;
                    var redditComment = comment.ToComment();
                    _service.RedditCommentDatabase.Upsert(redditComment);
                }

                Console.WriteLine($"Post {post.Id} had {comments.Count} comments");
            }

            Console.WriteLine($"Pulled {commentCount} comments from {postCount} posts");
        }
    }
}
