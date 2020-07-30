using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using System;

namespace ChefKnivesBotLib
{
    public class Initializer
    {
        public static ChefKnivesListener Start(ILogger logger, string settingsFileLocation)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(settingsFileLocation, false, true)
                .Build();

            logger.Information("Application started...");

            var reddit = new RedditClient(appId: configuration["AppId"], appSecret: configuration["AppSecret"], refreshToken: configuration["RefreshToken"]);

            var listener = new ChefKnivesListener(logger, reddit);

            return listener;
        }
    }
}
