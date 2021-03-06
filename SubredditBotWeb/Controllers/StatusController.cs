﻿using SubredditBot.Lib;
using SubredditBotWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ChefKnivesBot;
using System;

namespace SubredditBotWeb.Controllers
{
#if DEBUG
    [AllowAnonymous]
#else
    [Authorize(Policy = "SubredditModerator")]
#endif
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
                Message = $"Uptime: {DateTime.Now - Program.StartTime}"
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult RestartService()
        {
            Program.ChefKnivesService.Dispose();
            var chefKnivesBotInitializer = new ChefKnivesBotInitializer();
            Program.ChefKnivesService = chefKnivesBotInitializer.Start(Log.Logger, _configuration, Program.DiscordService.SendModChannelMessage, Program.DryRun);
            return RedirectToAction(nameof(Index));
        }
    }
}
