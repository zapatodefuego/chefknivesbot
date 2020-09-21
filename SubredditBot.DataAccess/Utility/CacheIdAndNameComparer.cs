using SubredditBot.Data;
using System.Collections.Generic;

namespace SubredditBot.DataAccess.Utility
{
    internal class CacheIdAndNameComparer<T> : IEqualityComparer<T> where T : RedditThing
    {
        public bool Equals(T x, T y)
        {
            return x.Id.Equals(y.Id) && x.Fullname.Equals(y.Fullname);
        }

        public int GetHashCode(T obj)
        {
            return obj.Id.GetHashCode() + obj.Fullname.GetHashCode();
        }
    }
}