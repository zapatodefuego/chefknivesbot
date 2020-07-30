using Reddit.Controllers;

namespace ChefKnivesBotLib.Handlers
{
    public interface ICommentHandler
    {
        void Process(Comment comment);
    }
}
