using Microsoft.Extensions.Configuration;
using Reddit;
using Serilog;
using SubredditBot.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChefKnivesBot
{
    public class ChefKnivesBotInitializer
    {
        private const string _subredditName = "chefknives";

        public SubredditService Start(ILogger logger, IConfiguration configuration, Func<string, Task> callback, bool dryRun = false)
        {
            logger.Information("Application started...");

            if (dryRun)
            {
                logger.Warning("This is a DRYRUN! No actions will be taken!");
            }

            var redditClient = new RedditClient(appId: configuration["AppId"], appSecret: configuration["AppSecret"], refreshToken: configuration["RefreshToken"]);
            var service = new SubredditService(logger, configuration, redditClient, subredditName: _subredditName, databaseName: _subredditName, callback : callback);

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

        private IEnumerable<dynamic> GetHandlers(Type type, ILogger logger, SubredditService service, bool dryRun)
        {
            var handlers = GetType().Assembly.GetTypes()
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList();

            foreach (var handler in handlers)
            {
                yield return Activator.CreateInstance(handler, new object[] { logger, service, dryRun });
            }
        }
    }
}
