using Reddit.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChefKnivesBotLib
{
    public static class Diagnostics
    {
        private static DateTime StartTime { get; set; }

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
            Reset();
        }

        public static void Reset()
        {
            StartTime = DateTime.Now;
            RedditServiceUnavailableExceptionCount = 0;
            OtherExceptionCount = 0;
            SeenComments = 0;
            ProcessedComments = 0;
            SeenPosts = 0;
            ProcessedPosts = 0;
            SeenMessages = 0;
            ProcessedMessages = 0;
        }

        public static string GetStatusMessage()
        {
            var uptime = DateTime.Now - StartTime;
            return $"Uptime: {uptime}\n\n " +
                $"Comments: saw {SeenComments}, processed {ProcessedComments}\n\n" +
                $"Posts: saw {SeenPosts}, processed {ProcessedPosts}\n\n" +
                $"Messages: saw {SeenMessages}, processed {ProcessedMessages}\n\n" +
                $"Exceptions: {nameof(RedditServiceUnavailableException)} {RedditServiceUnavailableExceptionCount}, other {OtherExceptionCount}";
        }
    }
}
