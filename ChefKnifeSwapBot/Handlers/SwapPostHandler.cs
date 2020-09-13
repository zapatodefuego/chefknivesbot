using Reddit.Controllers;
using Reddit.Things;
using Serilog;
using SubredditBot.Data;
using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChefKnifeSwapBot.Handlers
{
    public class SwapPostHandler : HandlerBase, IPostHandler
    {
        private const int _numEntrys = 18;
        private const string _titleEntry = "Selling table. All items mandatory. One table per post. This header must be included";
        private const string _nameEntry = "Item Name(s)";
        private const string _descriptionEntry = "Description(s)";
        private const string _priceEntry = "Asking price(s)";
        private const string _shippingEntry = "Shipping included in price?";
        private const string _regionEntry = "Region";
        private const string _productEntry = "Product page link(s)";
        private const string _pictureEntry = "Picture album link";
        private const string _endEntry = "End";
        private static string _table =
            $";{_titleEntry};\n" +
            $";{_nameEntry};\n" +
            $";{_descriptionEntry};\n" +
            $";{_priceEntry};\n" +
            $";{_shippingEntry};\n" +
            $";{_regionEntry};\n" +
            $";{_productEntry};\n" +
            $";{_pictureEntry};\n" +
            $";{_endEntry};\n";

        private static Regex _tableIdentifierRegex = new Regex(";", RegexOptions.Compiled);

        private ILogger _logger;
        private readonly SubredditService _service;
        private readonly FlairV2 _flair;

        public SwapPostHandler(ILogger logger, SubredditService service, bool dryRun)
            : base(dryRun)
        {
            _logger = logger;
            _service = service;
            _flair = service.Subreddit.Flairs.LinkFlairV2.First(f => f.Text.Equals("Selling"));
        }

        public bool Process(BaseController controller)
        {
            var post = controller as SelfPost;
            if (post == null)
            {
                return false;
            }

            if (post.Title.Contains("[Selling]", StringComparison.OrdinalIgnoreCase)|| (_flair != null && _flair.Equals(_flair.Id)))
            {
                // Set the flair
                post.SetFlair(_flair.Text, _flair.Id);

                if (_service.SelfCommentDatabase.GetBy(nameof(SelfComment.ParentId), post.Id).Result.Any())
                {
                    return false;
                }

                var body = post.SelfText;
                var tableMatches = _tableIdentifierRegex.Matches(body);
                if (tableMatches.Count == 0)
                {
                    if (!DryRun)
                    {
                        var response = $"It looks like your Selling post is missing the required table or it is formatted incorrectly. Please submit a new post containing the following table or [click this link to find out more](https://www.reddit.com/r/chefknifeswap/comments/irpqd2/we_will_be_testing_out_new_bot_functions_over_the/):\n\n {_table}";
                        var reply = post.Reply(response).Distinguish("yes", true);
                        //post.Remove();

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    }
                    return true;
                }
                else if(tableMatches.Count < _numEntrys)
                {
                    if (!DryRun)
                    {
                        var response = $"It looks like your Selling post does not contain all of the expected entries. Please submit a new post containing the following table or [click this link to find out more](https://www.reddit.com/r/chefknifeswap/comments/irpqd2/we_will_be_testing_out_new_bot_functions_over_the/):\n\n {_table}";
                        var reply = post.Reply(response).Distinguish("yes", true);
                        //post.Remove();

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    }

                    return true;
                }
                else if (tableMatches.Count > _numEntrys)
                {
                    if (!DryRun)
                    {
                        var response = $"It looks like your Selling post containes too many entries. Please submit a new post containing the following table or [click this link to find out more](https://www.reddit.com/r/chefknifeswap/comments/irpqd2/we_will_be_testing_out_new_bot_functions_over_the/):\n\n {_table}";
                        var reply = post.Reply(response).Distinguish("yes", true);
                        //post.Remove();

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    }

                    return true;
                }

                var hasError = false;
                var errorResponse = new StringBuilder();
                errorResponse.AppendLine("The following issues were found in your submission:");

                var parts = body.Split(';').Select(s => s.ToLower()).ToArray();
                if (!GetEntry(parts, _titleEntry, out string titleValue))
                {
                    errorResponse.AppendLine("* Title entry was modified, missing, or incorrectly formatted");
                    hasError = true;
                }

                if (!GetEntry(parts, _nameEntry, out string nameValue))
                {
                    errorResponse.AppendLine("* Item name entry was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetEntry(parts, _descriptionEntry, out string descriptionValue))
                {
                    errorResponse.AppendLine("* Description entry was missing or incorrectly formatted.");
                    hasError = true;
                }
                else if (descriptionValue.Length < 50)
                {
                    errorResponse.AppendLine("* C'mon you can write more of a description then that...");
                    hasError = true;
                }

                if (!GetEntry(parts, _priceEntry, out string priceValue))
                {
                    errorResponse.AppendLine("* Price entry was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetEntry(parts, _shippingEntry, out string shippingValue))
                {
                    errorResponse.AppendLine("* Shipping entry was missing or incorrectly formatted.");
                    hasError = true;
                }
                else if (!new string[] { "yes", "no" }.Any(s => shippingValue.Contains(s.ToLower())))
                {
                    errorResponse.AppendLine("* Shipping entry value should be \"yes\" or \"no\".");
                    hasError = true;
                }

                if (!GetEntry(parts, _regionEntry, out string regionValue))
                {
                    errorResponse.AppendLine("* Region entry was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetEntry(parts, _productEntry, out string productValue))
                {
                    errorResponse.AppendLine("* The product link entry was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetEntry(parts, _pictureEntry, out string pictureValue))
                {
                    errorResponse.AppendLine("* The album link entry was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (hasError)
                {
                    errorResponse.AppendLine("\n\nThis post has [NOT] been removed. Please correct the above issues and resubmit. [Click this link to find out more.](https://www.reddit.com/r/chefknifeswap/comments/irpqd2/we_will_be_testing_out_new_bot_functions_over_the/)");

                    if (!DryRun)
                    {
                        var reply = post
                        .Reply(errorResponse.ToString())
                        .Distinguish("yes", true);

                        //post.Remove();
                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    }

                    return true;
                }

                if (!DryRun)
                {
                    var postHistory = _service.RedditPostDatabase.GetByAuthor(post.Author).Result;

                    var replyMessage = new StringBuilder();
                    replyMessage.AppendLine($"I've reviewed this post and it looks good. However, I'm a new bot and am not great at my job yet. " +
                    "Please message the moderators if you have any feedback to offer. Do not respond to this comment since no one will see it.");

                    if (postHistory != null && !postHistory.Any())
                    {
                        replyMessage.AppendLine($"u/{post.Author} has not submitted any [Selling] posts in r/{_service.Subreddit.Name} since I've gained sentience");
                    }
                    else
                    {
                        replyMessage.AppendLine("Here are some past selling posts from u/{post.Author}");
                        postHistory.Take(5).ToList()
                            .ForEach(p => replyMessage.AppendLine($"* [{p.Title}]({_service.Subreddit.URL})/{p.Id}"));
                    }

                    var reply = post
                        .Reply(replyMessage.ToString())
                        .Distinguish("yes", true);

                    _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    _service.RedditPostDatabase.Upsert(post.ToPost());
                }

            }

            return false;
        }

        private bool GetEntry(string[] parts, string name, out string value)
        {
            var index = Array.IndexOf(parts, name.ToLower());
            if (index + 1 < parts.Length)
            {
                value = parts[index+1];
                return true;
            }

            value = null;
            return false;
        }
    }
}
