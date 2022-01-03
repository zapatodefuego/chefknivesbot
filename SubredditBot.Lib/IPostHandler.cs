using Reddit.Controllers;
using System;
using System.Threading.Tasks;

namespace SubredditBot.Lib
{
    public interface IPostHandler
    {
        /// <summary>
        /// Processess the controller
        /// </summary>
        /// <param name="controller">The post as a controller</param>
        /// <param name="callback">Callback for handling a logged string</param>
        /// <returns>True if the controller was processed, else false</returns>
        Task<bool> Process(BaseController controller, Func<string, Task> callback = null);
    }
}
