using SubredditBot.Data;
using System.Collections.Generic;

namespace SubredditBot.DataAccess.Utility
{
    internal class CacheAllPropertiesComparer<T> : IEqualityComparer<T> where T : RedditThing
    {
        public bool Equals(T x, T y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}