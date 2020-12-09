using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubredditBot.DataAccess
{
    public static class BsonTypeMapInitializer
    {
        private static bool _executed = false;
        public static void RunOnce()
        {
            if (!_executed)
            {
                BsonClassMap.RegisterClassMap<SubredditBot.Data.Comment>();
                BsonClassMap.RegisterClassMap<SubredditBot.Data.Post>();
                BsonClassMap.RegisterClassMap<SubredditBot.Data.SelfComment>();

                _executed = true;
            }
        }
    }
}
