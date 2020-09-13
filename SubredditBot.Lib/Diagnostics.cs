using Reddit.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SubredditBot.Lib
{
    public static class Diagnostics
    {

        private static DateTime StartTime { get; set; }

        public static int RedditServiceUnavailableExceptionCount { get; set; }

        public static int RedditGatewayTimeoutException { get; set; }

        public static int OtherExceptionCount { get; set; }

        public static int SeenComments { get; set; }

        public static int ProcessedComments { get; set; }

        public static int SeenPosts { get; set; }

        public static int ProcessedPosts { get; set; }

        public static int SeenMessages { get; set; }

        public static int ProcessedMessages { get; set; }

        public static List<long> ReviewTimes { get; set; }

        static Diagnostics()
        {
            Reset();
        }

        public static void Reset()
        {
            StartTime = DateTime.Now;
            RedditGatewayTimeoutException = 0;
            RedditServiceUnavailableExceptionCount = 0;
            OtherExceptionCount = 0;
            SeenComments = 0;
            ProcessedComments = 0;
            SeenPosts = 0;
            ProcessedPosts = 0;
            SeenMessages = 0;
            ProcessedMessages = 0;
            ReviewTimes = new List<long>();
        }

        public static void AddReviewTime(long time)
        {
            ReviewTimes.Add(time);
        }

        public static string GetStatusMessage()
        {
            var uptime = DateTime.Now - StartTime;
            return $"Uptime: {uptime}\n\n " +
                $"Comments: saw {SeenComments}, processed {ProcessedComments}\n\n" +
                $"Posts: saw {SeenPosts}, processed {ProcessedPosts}\n\n" +
                $"Messages: saw {SeenMessages}, processed {ProcessedMessages}\n\n" +
                $"Exceptions: {nameof(RedditServiceUnavailableException)} {RedditServiceUnavailableExceptionCount}, {nameof(RedditGatewayTimeoutException)} {RedditGatewayTimeoutException}, other {OtherExceptionCount}\n\n" +
                $"Average review time (ms): {(ReviewTimes.Any() ? ReviewTimes.Average() : 0)}";
        }
    }
}
