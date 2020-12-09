using ChefKnivesBotWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChefKnivesBotWeb.Controllers
{
    //[Authorize(Policy = "SubredditModerator")]
    [AllowAnonymous]
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
                model.Message = Program.ChefKnivesService.ReviewUser(model.Username);
            }

            return View(model);
        }
    }
}
