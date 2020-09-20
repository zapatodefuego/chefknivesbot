using SubredditBot.Lib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using ChefKnivesBot;
using ChefKnifeSwapBot;
using RykyBot;
using System.Collections.Generic;
using CuttingBoardsBot;

namespace SubredditBotWeb
{
    public class Program
    {
        public static SubredditService ChefKnivesService { get; set; }

        public static SubredditService ChefKnifeSwapService { get; set; }

        public static bool DryRun { get; private set; }

        private static IConfigurationRoot _configuration;

        public static void Main(string[] args)
        {
            // Base config
            var initialConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            // ChefKnives config
            var chefKnivesSettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["ChefKnivesSettingsFile"]);
            var compoundConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false);

            if (!string.IsNullOrEmpty(chefKnivesSettingsFile))
            {
                compoundConfiguration.AddJsonFile(chefKnivesSettingsFile, false, false);
            }

            _configuration = compoundConfiguration.Build();

            // Ryky config
            var rykySettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["RykySettingsFile"]);
            var rykyConfigBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, false);

            if (!string.IsNullOrEmpty(rykySettingsFile))
            {
                rykyConfigBuilder.AddJsonFile(rykySettingsFile, false, false);
            }

            var rykyConfig = rykyConfigBuilder.Build();

            // CuttingBoards config
            var cuttingBoardsSettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["CuttingBoardsSettingsFile"]);
            var cuttingBoardsConfigBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, false);

            if (!string.IsNullOrEmpty(cuttingBoardsSettingsFile))
            {
                cuttingBoardsConfigBuilder.AddJsonFile(cuttingBoardsSettingsFile, false, false);
            }

            var cuttingBoardsConfig = cuttingBoardsConfigBuilder.Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            if (!args.Any(a => a.Equals("--websiteonly")))
            {
                DryRun = args.Any(a => a.Equals("--dryrun"));

                ChefKnivesService = new ChefKnivesBotInitializer().Start(Log.Logger, _configuration, DryRun);
                ChefKnifeSwapService = new ChefKnifeSwapBotInitializer().Start(Log.Logger, _configuration, DryRun);
                var rykyService = new RykyBotInitializer().Start(Log.Logger, rykyConfig, DryRun);
                var cuttingBoardsService = new CuttingBoardsBotInitializer().Start(Log.Logger, cuttingBoardsConfig, DryRun);
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(Log.Logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseConfiguration(_configuration);
                });
    }
}