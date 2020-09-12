using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using SubredditBot.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChefKnivesBot
{
    public class Initializer
    {
        private const string _subredditName = "chefknives";

        public static SubredditService Start(ILogger logger, IConfiguration configuration, bool dryRun = false)
        {
            logger.Information("Application started...");

            if (dryRun)
            {
                logger.Warning("This is a DRYRUN! No actions will be taken!");
            }

            var redditClient = new RedditClient(appId: configuration["AppId"], appSecret: configuration["AppSecret"], refreshToken: configuration["RefreshToken"]);
            var subreddit = redditClient.Account.MyModeratorSubreddits().First(s => s.Name.Equals(_subredditName));
            var account = redditClient.Account;
            var makerPostFlair = subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Maker Post"));

            var service = new SubredditService(logger, configuration, redditClient, subreddit, account);

            foreach (var handler in GetHandlers(typeof(IPostHandler), logger, service, dryRun))
            {
                service.PostHandlers.Add(handler);
            }

            foreach (var handler in GetHandlers(typeof(ICommentHandler), logger, service, dryRun))
            {
                service.CommentHandlers.Add(handler);
            }

            foreach (var handler in GetHandlers(typeof(IMessageHandler), logger, service, dryRun))
            {
                service.MessageHandlers.Add(handler);
            }

            service.SubscribeToPostFeed();
            service.SubscribeToCommentFeed();
            service.SubscribeToMessageFeed();

            service.RegisterRepeatForCommentAndPostDataPull();

            return service;
        }

        private static IEnumerable<dynamic> GetHandlers(Type type, ILogger logger, SubredditService service, bool dryRun)
        {
            var handlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList();

            foreach (var handler in handlers)
            {
                yield return Activator.CreateInstance(handler, new object[] { logger, service, dryRun });
            }
        }
    }
}
