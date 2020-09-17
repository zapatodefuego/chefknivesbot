using SubredditBotWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChefKnivesBot.Utilities;

namespace SubredditBotWeb.Controllers
{
    [Authorize(Policy = "SubredditModerator")]
    public class TestPageController : Controller
    {
        public TestPageController()
        {

        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new TestPageModel
            {
                Message = "Well, do something first..."
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(TestPageModel model)
        {
            if (!string.IsNullOrEmpty(model.Username))
            {
                model.Message = ReviewUser(model.Username);
            }

            return View(model);
        }

        private string ReviewUser(string username)
        {
            var reviewTask = MakerCommentsReviewUtility.Review(username, Program.ChefKnivesService.RedditPostDatabase, Program.ChefKnivesService.RedditCommentDatabase);
            var result = reviewTask.GetAwaiter().GetResult();

            return $"SelfPostComments: {result.SelfPostComments}, OtherComments: {result.OtherComments}, ReviewTime: {result.ReviewTime} (ms), Error: {result.Error}";
        }
    }
}
