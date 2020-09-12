using SubredditBot.Lib;
using SubredditBotWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ChefKnivesBot;

namespace SubredditBotWeb.Controllers
{
    [Authorize(Policy = "SubredditModerator")]
    public class StatusController : Controller
    {
        private readonly IConfiguration _configuration;

        public StatusController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new StatusModel
            {
                Message = Diagnostics.GetStatusMessage()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult RestartService()
        {
            Diagnostics.Reset();
            Program.ChefKnivesService.Dispose();
            Program.ChefKnivesService = Initializer.Start(Log.Logger, _configuration, Program.DryRun);
            return RedirectToAction(nameof(Index));
        }
    }
}
