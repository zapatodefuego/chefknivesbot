using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using System;
using System.Linq;
using Account = Reddit.Controllers.Account;
using Post = Reddit.Controllers.Post;

namespace ChefKnivesBotLib.Handlers.Posts
{
    public class MakerPostHandler : HandlerBase, IControllerHandler
    {
        private readonly ILogger _logger;
        private FlairV2 _makerPostFlair;
        private Account _account;

        public MakerPostHandler(ILogger logger, FlairV2 makerPostFlair, Account account, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _makerPostFlair = makerPostFlair;
            _account = account;
        }
        public bool Process(BaseController baseController)
        {
            var post = baseController as Post;
            if (post == null)
            {
                return false;
            }

            var linkFlairId = post.Listing.LinkFlairTemplateId;

            // Check that the tile contains [maker post] or that the link flair matches the maker post flair (does the work for updates? i don't know yet)
            if (post.Title.Contains("[maker post]", StringComparison.OrdinalIgnoreCase)
                    || (linkFlairId != null && linkFlairId.Equals(_makerPostFlair.Id))
               )
            {
                // Set the flair
                post.SetFlair(_makerPostFlair.Text, _makerPostFlair.Id);

                // Check if we already commented on this post
                if (!post.Comments.New.Any(c => c.Author.Equals(_account.Me.Name) && c.Body.StartsWith("This post has been identified")))
                {
                    if (!DryRun)
                    {
                        post
                            .Reply(
                                "This post has been identified as a maker post! If you have not done so please review the [Maker FAQ](https://www.reddit.com/r/chefknives/wiki/makerfaq). \n\n " +
                                "As a reminder to all readers, you may not discuss sales, pricing, for OP to make you something, or where to buy what OP is displaying or similar in this thread or anywhere on r/chefknives. Use private messages for any such communication. \n\n " +
                                "^(I am a bot. Beep boop.)")
                            .Distinguish("yes", true);
                    }

                    _logger.Information($"Commented with maker warning on post by {post.Author}");
                }

                return true;
            }

            return false;
        }
    }
}
