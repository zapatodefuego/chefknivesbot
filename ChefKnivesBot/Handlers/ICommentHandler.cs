using Reddit.Controllers;

namespace ChefKnivesBot.Handlers
{
    public interface ICommentHandler
    {
        /// <summary>
        /// Processess the controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if the controller was processed, else false</returns>
        bool Process(BaseController controller);
    }
}
