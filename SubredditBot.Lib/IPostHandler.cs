using Reddit.Controllers;
using System.Threading.Tasks;

namespace SubredditBot.Lib
{
    public interface IPostHandler
    {
        /// <summary>
        /// Processess the controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if the controller was processed, else false</returns>
        Task<bool> Process(BaseController controller);
    }
}
