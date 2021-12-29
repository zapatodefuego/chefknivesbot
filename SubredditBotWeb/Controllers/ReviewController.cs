using SubredditBotWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChefKnivesBot.Utilities;
using System.Linq;

namespace SubredditBotWeb.Controllers
{
#if DEBUG
    [AllowAnonymous]
#else
    [Authorize(Policy = "SubredditModerator")]
#endif
    public class ReviewController : Controller
    {
        private const string urlRoot = "https://www.reddit.com";

        public ReviewController()
        {

        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ReviewModel());
        }

        [HttpPost]
        public IActionResult Index(ReviewModel model)
        {
            if (!string.IsNullOrEmpty(model.Username))
            {
                ReviewUser(model);
            }

            return View(model);
        }

        private void ReviewUser(ReviewModel model)
        {
            var reviewTask = MakerCommentsReviewUtility.Review(model.Username, Program.ChefKnivesService.RedditPostDatabase, Program.ChefKnivesService.RedditCommentDatabase);
            var result = reviewTask.GetAwaiter().GetResult();

            model.Message = $"Reviewed {model.Username} in {result.ReviewTime} (ms). Found {result.MakerPosts.Count()} maker posts and {result.Comments.Count() - result.SelfComments.Count()} total non-maker comments.";
            model.Comments = result.Comments.OrderByDescending(o => o.CreateDate).ToList();
            model.Comments.ForEach(c => c.Permalink = $"{urlRoot}{c.Permalink}");

            model.Posts = result.Posts.OrderByDescending(o => o.CreateDate).ToList();
            model.Posts.ForEach(p => p.Permalink = $"{urlRoot}{p.Permalink}");
        }
    }
}
