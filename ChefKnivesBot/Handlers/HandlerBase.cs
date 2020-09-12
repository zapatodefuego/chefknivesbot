using System;
using System.Collections.Generic;
using System.Text;

namespace ChefKnivesBot.Handlers
{
    public abstract class HandlerBase
    {
        public bool DryRun { get; }

        public HandlerBase(bool dryRun)
        {
            DryRun = dryRun;
        }
    }
}
