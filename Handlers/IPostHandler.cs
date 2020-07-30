using Reddit.Controllers;

namespace ChefKnivesBot.Handlers
{
    public interface IPostHandler
    {
        void Process(Post post);
    }
}
