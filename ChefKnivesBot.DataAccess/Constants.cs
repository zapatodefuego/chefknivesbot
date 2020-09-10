using System;

namespace ChefKnivesBot.DataAccess
{
    public static class DatabaseConstants
    {
        public const string CommentsCollectionName = "comments";

        public const string PostsCollectionName = "posts";

        public const string SelfCommentsCollectionName = "selfcomments";

        public static DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
