using Reddit.Things;
using Serilog;
using System;
using System.Linq;
using Post = Reddit.Controllers.Post;
using User = Reddit.Controllers.User;

namespace ChefKnivesBot.Handlers.Posts
{
    public class MakerPostHandler : IPostHandler
    {
        private readonly ILogger _logger;
        private FlairV2 _makerPostFlair;
        private User _me;

        public MakerPostHandler(ILogger logger, FlairV2 makerPostFlair, User me)
        {
            _logger = logger;
            _makerPostFlair = makerPostFlair;
            _me = me;
        }
        public void Process(Post post)
        {
            var linkFlairId = post.Listing.LinkFlairTemplateId;

            // Check that the tile contains [maker post] or that the link flair matches the maker post flair (does the work for updates? i don't know yet)
            if (post.Title.Contains("[maker post]", StringComparison.OrdinalIgnoreCase)
                    || (linkFlairId != null && linkFlairId.Equals(_makerPostFlair.Id))
               )
            {
                // Set the flair
                post.SetFlair(_makerPostFlair.Text, _makerPostFlair.Id);

                // Check if we already commented on this post
                if (!post.Comments.New.Any(c => c.Author.Equals(_me.Name) && c.Body.StartsWith("This post has been identified")))
                {
                    post
                        .Reply(
                            "This post has been identified as a maker post! If you have not done so please review the [Maker FAQ](https://www.reddit.com/r/chefknives/wiki/makerfaq). \n\n " +
                            "As a reminder to all readers, you may not discuss sales, pricing, for OP to make you something, or where to buy what OP is displaying or similar in this thread or anywhere on r/chefknives. Use private messages for any such communication. \n\n " +
                            "^(I am a bot. Beep boop.)")
                        .Distinguish("yes", true);

                    _logger.Information($"Commented with maker warning on post by {post.Author}");
                }
            }
        }
    }
}
