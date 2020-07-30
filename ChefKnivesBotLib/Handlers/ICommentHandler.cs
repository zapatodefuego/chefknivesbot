using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChefKnivesBot.Handlers
{
    public interface ICommentHandler
    {
        void Process(Comment comment);
    }
}
