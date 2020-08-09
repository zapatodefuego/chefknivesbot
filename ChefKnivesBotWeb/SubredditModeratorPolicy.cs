using Microsoft.AspNetCore.Authorization;

namespace ChefKnivesBotWeb
{
    public class SubredditModeratorPolicy : IAuthorizationRequirement
    {
        public string SubredditName { get; }

        public SubredditModeratorPolicy(string subredditName)
        {
            SubredditName = subredditName;
        }
    }
}