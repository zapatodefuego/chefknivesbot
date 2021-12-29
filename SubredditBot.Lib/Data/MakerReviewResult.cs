using SubredditBot.Data;
using System.Collections.Generic;

namespace SubredditBot.Lib.Data
{
    public class MakerReviewResult
    {
        public long ReviewTime { get; set; }
        public IEnumerable<Post> MakerPosts { get; set; }
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public IEnumerable<Comment> SelfComments { get; set; }
    }

}
