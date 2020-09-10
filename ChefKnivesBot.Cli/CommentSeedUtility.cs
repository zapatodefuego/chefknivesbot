using ChefKnivesBot.Lib;
using ChefKnivesBot.Lib.DataExtensions;
using System;
using System.Threading.Tasks;

namespace ChefKnivesBot.Cli
{
    public class CommentSeedUtility
    {
        private readonly ChefKnivesService _service;

        public CommentSeedUtility(ChefKnivesService service)
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
                var comments = _service.RedditClient.Post($"t3_{post.Id}").Comments.GetComments();
                foreach (var comment in comments)
                {
                    commentCount++;
                    var redditComment = comment.ToRedditComment();
                    _service.RedditCommentDatabase.Upsert(redditComment);
                }

                Console.WriteLine($"Post {post.Id} had {comments.Count} comments");
            }

            Console.WriteLine($"Pulled {commentCount} comments from {postCount} posts");
        }
    }
}
