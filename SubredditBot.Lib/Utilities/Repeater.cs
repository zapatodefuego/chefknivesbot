using System;
using System.Threading;
using System.Threading.Tasks;

namespace SubredditBot.Lib.Utilities
{
    public static class Repeater
    {
        public static void Repeat(Action action, int seconds, CancellationToken token)
        {
            if (action == null)
            {
                return;
            }

            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                }
            }, token);
        }
    }
}
