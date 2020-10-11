using SubredditBot.Data;
using System.Collections.Generic;

namespace SubredditBotWeb.Models
{
    public class ReviewModel
    {
        public List<Comment> Comments { get; set; }

        public List<Post> Posts { get; set; }

        public string Message { get; set; }

        public string Username { get; set; }
    }
}
