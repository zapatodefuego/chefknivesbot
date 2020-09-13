using SubredditBot.Lib;
using SubredditBot.Lib.DataExtensions;
using Reddit.Inputs;
using System;
using System.Threading.Tasks;

namespace SubredditBot.Cli
{
    public class PostSeedUtility
    {
        private readonly SubredditService _service;

        public PostSeedUtility(SubredditService service)
        {
            _service = service;
        }

        public async Task Execute()
        {
        }
    }
}
