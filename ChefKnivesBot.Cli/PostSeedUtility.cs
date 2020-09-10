using ChefKnivesBot.Lib;
using ChefKnivesBot.Lib.DataExtensions;
using Reddit.Inputs;
using System;
using System.Threading.Tasks;

namespace ChefKnivesBot.Cli
{
    public class PostSeedUtility
    {
        private readonly ChefKnivesService _service;

        public PostSeedUtility(ChefKnivesService service)
        {
            _service = service;
        }

        public async Task Execute()
        {
        }
    }
}
