using Reddit.Controllers;
using System;
using System.Threading.Tasks;

namespace SubredditBot.Lib
{
    public interface ICommentHandler
    {
        /// <summary>
        /// Processess the controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if the controller was processed, else false</returns>
        Task<bool> Process(BaseController controller, Func<string, Task> callback);
    }
}
