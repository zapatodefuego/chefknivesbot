using Reddit.Controllers;

namespace ChefKnivesBotLib.Handlers
{
    public interface IPostHandler
    {
        void Process(Post post);
    }
}
