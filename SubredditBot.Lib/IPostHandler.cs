﻿using Reddit.Controllers;

namespace SubredditBot.Lib
{
    public interface IPostHandler
    {
        /// <summary>
        /// Processess the controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if the controller was processed, else false</returns>
        bool Process(BaseController controller);
    }
}
