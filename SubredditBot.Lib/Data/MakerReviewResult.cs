using System;
using System.Collections.Generic;
using System.Text;

namespace SubredditBot.Lib.Data
{
    public class MakerReviewResult
    {
        public int SelfPostComments { get; set; }

        public int OtherComments { get; set; }

        public string Error { get; set; }

        public long ReviewTime { get; set; }
    }

}
