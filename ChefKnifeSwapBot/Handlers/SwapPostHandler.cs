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
using System.Threading.Tasks;

namespace ChefKnifeSwapBot.Handlers
{
    public class SwapPostHandler : HandlerBase, IPostHandler
    {
        private const string _postUrlFirstPart = "https://www.reddit.com/r/chefknifeswap/comments/";
        private const string _nameKey = "name:";
        private const string _regionKey = "region:";
        private const string _priceKey = "price:";
        private const string _picturesKey = "pictures:";
        private string[] _keys = { _nameKey, _regionKey, _priceKey, _picturesKey };

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

        public async Task<bool> Process(BaseController controller, Func<string, Task> callback = null)
        {
            var post = controller as SelfPost;
            if (post == null)
            {
                return false;
            }

            var result = await _service.SelfCommentDatabase.GetAny(nameof(SelfComment.ParentId), post.Id);
            if (result != null)
            {
                return false;
            }

            var linkFlair = post.Listing.LinkFlairTemplateId;
            var postShouldBeRemoved = false;
            var replyMessage = new StringBuilder();
            if (post.Title.Contains("[Selling]", StringComparison.OrdinalIgnoreCase) || (linkFlair != null && linkFlair.Equals(_flair.Id)))
            {
                // Set the flair
                if (!DryRun)
                {
                    post.SetFlair(_flair.Text, _flair.Id);
                }

                // process as selling post
                var content = Process(post.SelfText);
                var missingItems = _keys.Except(content.Keys);
                if (missingItems.Any())
                {
                    postShouldBeRemoved = true;

                    replyMessage.AppendLine("The following items were missing from your post:");
                    foreach (var missingItem in missingItems)
                    {
                        replyMessage.AppendLine($"* {GetMissingItemMessage(missingItem)}");
                    }

                    replyMessage.AppendLine("\nThis post as been removed. Please correct the above errors and resubmit. More information can be found in the wiki: https://www.reddit.com/r/chefknifeswap/about/wiki/index/selling_table \n");
                }
            } 

            var postHistory = _service.RedditPostDatabase.GetByFilter(nameof(RedditThing.Author), post.Author).Result;
            if (postHistory == null || !postHistory.Any())
            {
                replyMessage.AppendLine($"u/{post.Author} has not submitted any posts in r/{_service.Subreddit.Name} since I've gained sentience");
            }
            else
            {
                replyMessage.AppendLine($"Here are some past posts from u/{post.Author}:");
                postHistory.OrderByDescending(p => p.CreateDate).Take(5).ToList()
                    .ForEach(p => replyMessage.AppendLine($"* [{p.Title}]({_postUrlFirstPart}{p.Id})"));
            }

            if (!DryRun)
            {
                var reply = post
                    .Reply(replyMessage.ToString())
                    .Distinguish("yes", true);

                if (postShouldBeRemoved)
                {
                    post.Remove();
                }

                _service.SelfCommentDatabase.Upsert(reply.ToSelfComment(post.Id, RedditThingType.Post, post.Listing.LinkFlairTemplateId));
                _service.RedditPostDatabase.Upsert(post.ToPost());

                return true;
            }

            return false;
        }

        private string GetMissingItemMessage(string missingItem)
        {
            return (missingItem.ToLower()) switch
            {
                _nameKey => $"{_nameKey} - used to identify the item or items being sold.",
                _picturesKey => $"{_picturesKey} - used to provide a link to an image album on any image hosting website.",
                _regionKey => $"{_picturesKey} - used to specify which regions you are selling to, for example CONUS, EU, US, etc.",
                _priceKey => $"{_priceKey} - used to provide a price or prices for the items being sold.",
                _ => "Unknown error.",
            };
        }

        private Dictionary<string, string> Process(string content)
        {
            var keyIndexDictionary = new Dictionary<string, int>();
            var contentDictionary = new Dictionary<string, string>();
            foreach (var key in _keys)
            {
                var keyIndex = content.IndexOf(key, System.StringComparison.OrdinalIgnoreCase);
                if (keyIndex >= 0)
                {
                    keyIndexDictionary.Add(key, keyIndex);
                }
            }

            var orderedKeyIndexDictionary = keyIndexDictionary.OrderBy(kvp => kvp.Value).ToList();
            for (var i = 0; i < orderedKeyIndexDictionary.Count; i++)
            {
                string value = null;
                var startIndex = orderedKeyIndexDictionary[i].Value + orderedKeyIndexDictionary[i].Key.Length;

                if (i + 1 < orderedKeyIndexDictionary.Count)
                {
                    var length = orderedKeyIndexDictionary[i + 1].Value - startIndex;
                    value = content.Substring(startIndex, length).Trim();
                }
                else
                {
                    var length = content.IndexOf("\n", startIndex) - startIndex;
                    if (length >= 0)
                    {
                        value = content.Substring(startIndex, length).Trim();
                    }
                    else
                    {
                        value = content.Substring(startIndex).Trim();
                    }
                }

                contentDictionary.Add(orderedKeyIndexDictionary[i].Key, value);
            }

            return contentDictionary;
        }
    }
}
