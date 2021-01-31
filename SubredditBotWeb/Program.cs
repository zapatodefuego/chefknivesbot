using ChefKnifeSwapBot;
using ChefKnivesBot;
using ChefKnivesDiscord;
using CuttingBoardsBot;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SubredditBot.Lib;
using System;
using System.Linq;

namespace SubredditBotWeb
{
    public class Program
    {
        public static DateTime StartTime { get; } = DateTime.Now;

        public static DiscordService DiscordService { get; set; }

        public static SubredditService ChefKnivesService { get; set; }

        public static SubredditService ChefKnifeSwapService { get; set; }

        public static bool DryRun { get; private set; }

        public static IConfigurationRoot Configuration;

        public static void Main(string[] args)
        {
            // Base config
            var initialConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            // ChefKnives config
            //var chefKnivesSettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["ChefKnivesSettingsFile"]);
            //var compoundConfiguration = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json", false, false);

            //if (!string.IsNullOrEmpty(chefKnivesSettingsFile))
            //{
            //    compoundConfiguration.AddJsonFile(chefKnivesSettingsFile, false, false);
            //}

            Configuration = initialConfiguration;

            // Ryky config
            //var rykySettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["RykySettingsFile"]);
            //var rykyConfigBuilder = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json", true, false);

            //if (!string.IsNullOrEmpty(rykySettingsFile))
            //{
            //    rykyConfigBuilder.AddJsonFile(rykySettingsFile, false, false);
            //}

            //var rykyConfig = rykyConfigBuilder.Build();

            // CuttingBoards config
            //var cuttingBoardsSettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["CuttingBoardsSettingsFile"]);
            //var cuttingBoardsConfigBuilder = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json", true, false);

            //if (!string.IsNullOrEmpty(cuttingBoardsSettingsFile))
            //{
            //    cuttingBoardsConfigBuilder.AddJsonFile(cuttingBoardsSettingsFile, false, false);
            //}

            //var cuttingBoardsConfig = cuttingBoardsConfigBuilder.Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            DiscordService = new DiscordService(Log.Logger, Configuration.GetSection("ChefKnivesSettings"));
            DiscordService.Start().GetAwaiter().GetResult();

            if (!args.Any(a => a.Equals("--websiteonly")))
            {

                DryRun = false;
#if DEBUG
                DryRun = true;
#endif
                if (DryRun)
                {
                    Log.Logger.Fatal("This is a DRYRUN! No actions will be taken!");
                    Log.Logger.Fatal("This is a DRYRUN! No actions will be taken!");
                    Log.Logger.Fatal("This is a DRYRUN! No actions will be taken!");
                }

                ChefKnivesService = new ChefKnivesBotInitializer().Start(Log.Logger, Configuration.GetSection("ChefKnivesSettings"), DiscordService.SendModChannelMessage, DryRun);

                // TODO: processing old posts is disabled for swap since it causes the bot to run back in time and comment on many old posts, which we
                // probably don't want
                ChefKnifeSwapService = new ChefKnifeSwapBotInitializer().Start(Log.Logger, Configuration.GetSection("ChefKnifeSwapSettings"), dryRun: DryRun, processOldPosts: false);
                //var rykyService = new RykyBotInitializer().Start(Log.Logger, rykyConfig, DryRun);
                var cuttingBoardsService = new CuttingBoardsBotInitializer().Start(Log.Logger, Configuration.GetSection("CuttingBoardsSettings"), DryRun);
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(Log.Logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseConfiguration(Configuration);
                });
    }
}