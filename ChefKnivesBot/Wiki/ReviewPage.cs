using Reddit.Controllers;
using Serilog;
using SubredditBot.Lib;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChefKnivesBot.Wiki
{
    public class ReviewPage
    {
        private readonly ILogger _logger;
        private readonly ISubredditService _service;
        private readonly Subreddit _subreddit;

        public ReviewPage(ILogger logger, ISubredditService service)
        {
            _logger = logger;
            _service = service;
            _subreddit = service.Subreddit;
        }

        public void AddReviewLinkToReviewPage(string name, string author, string title, string url)
        {
            var reviewPage = GetReviewsPage();
            var content = reviewPage.ContentMd;
            var updatedContent = content;

            var regex = new Regex($"##.*", RegexOptions.IgnoreCase);
            var names = regex.Matches(content).Cast<Match>().ToList();

            names.Sort((a, b) => (a.Value.CompareTo(b.Value)));

            var matchingName = names.SingleOrDefault(n => n.Value.Equals($"##{name}", System.StringComparison.OrdinalIgnoreCase));
            if (matchingName != null)
            {
                var insertIndex = matchingName.Index + matchingName.Value.Length + 1;
                updatedContent = content.Insert(insertIndex, $"* [{title}]({url}) by {author}\n");
            }
            else
            {
                var highestSibling = names.FirstOrDefault(n => n.Value.CompareTo($"##{name}") > 0);
                if (highestSibling == null)
                {
                    var newEntry =
                        $"\n\n##{name}\n" +
                        $"* [{ title}]({ url}) by u/{ author}\n" +
                        "\n";
                    updatedContent = content.Insert(content.Length, newEntry);
                }
                else
                {
                    var newEntry = 
                        $"##{name}\n" +
                        $"* [{ title}]({ url}) by u/{ author}\n" +
                       "\n";
                    updatedContent = content.Insert(highestSibling.Index, newEntry);
                }
            }

            var result = Write(reviewPage, $"Adding review by {author}", updatedContent);
        }

        public WikiPage GetReviewsPage()
        {
            return _subreddit.Wiki.GetPage("reviews");
        }

        public WikiPage Write(WikiPage page, string reason, string content)
        {
            return page.EditAndReturn(reason, content, page.Revisions()[0].Id);
        }
    }
}
