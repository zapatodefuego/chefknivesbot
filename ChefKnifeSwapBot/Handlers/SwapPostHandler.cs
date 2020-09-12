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
using Comment = Reddit.Controllers.Comment;
using Post = Reddit.Controllers.Post;

namespace ChefKnifeSwapBot.Handlers
{
    public class SwapPostHandler : HandlerBase, IPostHandler
    {
        private const string _titleRow = "Selling table";
        private const string _nameRow = "Item Name(s)";
        private const string _descriptionRow = "Description(s)";
        private const string _priceRow = "Asking price(s)";
        private const string _shippingRow = "Shipping included in price?";
        private const string _regionRow = "Region";
        private const string _productRow = "Product page link(s)";
        private const string _pictureRow = "Picture album link";
        private static string _table =
            $"|{_titleRow }|All rows mandatory. One table per post. Do not edit these headers or the first row|\n" +
            $"|:-|:-|\n" +
            $"|{_nameRow}|<a name for the item being sold>|\n" +
            $"|{_descriptionRow}|<a description of the item>|\n" +
            $"|{_priceRow}|<asking price>|\n" +
            $"|{_shippingRow}|<yes or no>|\n" +
            $"|{_regionRow}|<us, eu, conus, uk and us, etc.>|\n" +
            $"|{_productRow}|<link to where the item is being sold or to a page about the maker if selling a custom item>|\n" +
            $"|{_pictureRow}|<link to an album of pictures. every picture must contain the timestamp>|\n";

        private static Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();
        private static Regex _tableIdentifierRegex = new Regex("\\|:-\\|:-\\|", RegexOptions.Compiled);

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
                        var response = $"It looks like your Selling post is missing the required table. Please submit a new post containing the following table:\n\n {_table}";
                        var reply = post.Reply(response).Distinguish("yes", true);
                        //post.Remove();

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    }
                    return true;
                }
                else if(tableMatches.Count > 1)
                {
                    if (!DryRun)
                    {
                        var response = "It looks like your Selling post contains multiple items. Only one item may be listed in each Selling post.";
                        var reply = post.Reply(response).Distinguish("yes", true);
                        //post.Remove();

                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    }

                    return true;
                }
                else if (body.Contains("<") || body.Contains(">"))
                {
                    if (!DryRun)
                    {
                        var reply = post.Reply("I found a \"<\" or a \">\" in your post. Did you forget to fill something out?").Distinguish("yes", true);
                        //post.Remove();
                        _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                    }

                    return true;
                }

                var hasError = false;
                var errorResponse = new StringBuilder();
                errorResponse.AppendLine("The following issues were found in your submission:");

                if (!GetRow(body, _titleRow, out string titleValue))
                {
                    errorResponse.AppendLine("* Title row was modified, missing, or incorrectly formatted");
                    hasError = true;
                }

                if (!GetRow(body, _nameRow, out string nameValue))
                {
                    errorResponse.AppendLine("* Item name row was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetRow(body, _descriptionRow, out string descriptionValue))
                {
                    errorResponse.AppendLine("* Description row was missing or incorrectly formatted.");
                    hasError = true;
                }
                else if (descriptionValue.Length < 50)
                {
                    errorResponse.AppendLine("* C'mon you can write more of a description then that...");
                    hasError = true;
                }

                if (!GetRow(body, _priceRow, out string priceValue))
                {
                    errorResponse.AppendLine("* Price row was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetRow(body, _shippingRow, out string shippingValue))
                {
                    errorResponse.AppendLine("* Shipping row was missing or incorrectly formatted.");
                    hasError = true;
                }
                else if (!new string[]{ "yes", "no" }.Any(shippingValue.Contains))
                {
                    errorResponse.AppendLine("* Shipping row value should be \"yes\" or \"no\".");
                    hasError = true;
                }

                if (!GetRow(body, _regionRow, out string regionValue))
                {
                    errorResponse.AppendLine("* Region row was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetRow(body, _productRow, out string productValue))
                {
                    errorResponse.AppendLine("* The product link row was missing or incorrectly formatted.");
                    hasError = true;
                }

                if (!GetRow(body, _pictureRow, out string pictureValue))
                {
                    errorResponse.AppendLine("* The album link row was missing or incorrectly formatted.");
                    hasError = true;
                }
                else if (Uri.TryCreate(pictureValue, UriKind.Absolute, out _))
                {
                    errorResponse.AppendLine("* The album link was not a valid url.");
                    hasError = true;
                }

                if (hasError)
                {
                    errorResponse.AppendLine("\n\nThis post has been removed. Please correct the above issues and resubmit.");

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
                    var reply = post
                    .Reply("I've reviewed this post and it looks good. However, I'm a new bot and am not great at my job. Please message the moderators if you have any feed to offer me. Do not respond to this comment since no one will see it.")
                    .Distinguish("yes", true);

                    //post.Remove();
                    _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post));
                }

            }

            return false;
        }

        private bool GetRow(string body, string name, out string value)
        {
            if (!_regexCache.TryGetValue(name, out Regex rowRegex))
            {
                rowRegex = new Regex(@$"\|{name}\|.*\|");
                _regexCache.Add(name, rowRegex);
            }

            var matches = rowRegex.Match(body);
            if (matches.Success)
            {
                value = matches.Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
