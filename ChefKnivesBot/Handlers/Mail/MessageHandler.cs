using Reddit.Inputs.LinksAndComments;
using Reddit.Things;
using Serilog;
using SubredditBot.Lib;

namespace ChefKnivesBot.Handlers.Mail
{
    public class MessageHandler : HandlerBase, IMessageHandler
    {
        private const string _status = "!status";

        private readonly ILogger _logger;
        private readonly ISubredditService _service;

        public MessageHandler(ILogger logger, ISubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
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
                _service.Account.Messages.Reply(new LinksAndCommentsThingInput(text, message.Id));
                return true;
            }

            return false;
        }
    }
 }
