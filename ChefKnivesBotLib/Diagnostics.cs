using Reddit.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChefKnivesBotLib
{
    public static class Diagnostics
    {
        private static DateTime StartTime { get; }
        public static int RedditServiceUnavailableExceptionCount { get; set; }
        public static int OtherExceptionCount { get; set; }

        public static int SeenComments { get; set; }

        public static int ProcessedComments { get; set; }

        public static int SeenPosts { get; set; }

        public static int ProcessedPosts { get; set; }

        public static int SeenMessages { get; set; }

        public static int ProcessedMessages { get; set; }

        static Diagnostics()
        {
            StartTime = DateTime.Now;
        }

        public static string GetStatusMessage()
        {
            var uptime = DateTime.Now - StartTime;
            return $"Uptime: {uptime}\n\n " +
                $"Comments: saw {SeenComments}, processed {ProcessedComments}\n\n" +
                $"Posts: saw {SeenPosts}, processed {ProcessedPosts}\n\n" +
                $"Messages: saw {SeenMessages}, processed {ProcessedMessages}\n\n" +
                $"Excetipsions: {nameof(RedditServiceUnavailableException)} {RedditServiceUnavailableExceptionCount}, other {OtherExceptionCount}";
        }
    }
}
