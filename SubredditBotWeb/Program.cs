using SubredditBot.Lib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using ChefKnivesBot;

namespace SubredditBotWeb
{
    public class Program
    {
        public static SubredditService ChefKnivesService { get; set; }

        public static bool DryRun { get; private set; }

        private static IConfigurationRoot _configuration;

        public static void Main(string[] args)
        {
            var initialConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            var redditSettingsFile = Environment.ExpandEnvironmentVariables(initialConfiguration["RedditSettingsFile"]);
            var compoundConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            if (!string.IsNullOrEmpty(redditSettingsFile))
            {
                compoundConfiguration.AddJsonFile(redditSettingsFile, true, false);
            }

            _configuration = compoundConfiguration.Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            if (!args.Any(a => a.Equals("--websiteonly")))
            {
                DryRun = args.Any(a => a.Equals("--dryrun"));
                ChefKnivesService = Initializer.Start(Log.Logger, _configuration, DryRun);
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