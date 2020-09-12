using Reddit.Controllers;
using SubredditBot.Lib;

namespace ChefKnifeSwapBot
{
    public class SwapPostHandler : HandlerBase, IPostHandler
    {
        private const string _table =
            "|r/chefkniveswap item sale table|All rows mandatory. One table per item|" +
            "|:-|:-|" +
            "|Item Name|<a name for the item being sold>|" +
            "|Description|<a description of the item>|" +
            "|Price|<asking price>|" +
            "|Shipping included in price?|<yes or no>|" +
            "|Region|<us|eu|conus|uk and us|etc.>|" +
            "|Product page link|<link to where the item is being sold or to a page about the maker if selling a custom item>|" +
            "|Pictures link|<link to an album of pictures>|" +
            "|Timestamp link|<direct link to a picture with the timestamp>|";

        public SwapPostHandler(bool dryRun) 
            : base(dryRun)
        {
        }

        public bool Process(BaseController controller)
        {
            throw new System.NotImplementedException();
        }
    }
}
