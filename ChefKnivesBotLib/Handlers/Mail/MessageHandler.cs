using Reddit;
using Reddit.Controllers;
using Reddit.Inputs.LinksAndComments;
using Reddit.Things;
using Serilog;

namespace ChefKnivesBotLib.Handlers.Mail
{
    public class MessageHandler : IThingHandler
    {
        private const string _status = "!status";

        private readonly ILogger _logger;
        private readonly RedditClient _redditClient;
        private readonly Account _account;

        public MessageHandler(ILogger logger, RedditClient redditClient, Account account)
        {
            _logger = logger;
            _redditClient = redditClient;
            _account = account;
        }

        public bool Process(object thing)
        {
            var message = thing as Message;
            if (message == null)
            {
                return false;
            }

            if (message.Body.Equals(_status))
            {
                var text = Diagnostics.GetStatusMessage();
                _account.Messages.Reply(new LinksAndCommentsThingInput(text, message.Id));
                return true;
            }

            return false;
        }
    }
 }
