using SubredditBot.Data;

namespace ChefKnivesBot.Utilities
{
    public class SelfPostUtilities
    {
        public static bool PostHasExistingResponse(SelfComment selfComment, string flairId)
        {
            if (selfComment == null)
            {
                return false;
            }
            else if (selfComment.ParentFlairId != null && selfComment.ParentFlairId == flairId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
