using SubredditBot.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SubredditBot.DataAccess.Utility
{
    /// <summary>
    /// Cache of the most recent items added
    /// </summary>
    public class DatabaseCache<T> : IEnumerable, IDisposable where T : RedditThing
    {
        private readonly ISet<T> _set = new HashSet<T>();
        private readonly LinkedList<T> _list = new LinkedList<T>();
        public int MaxSize { get; set; }

        public DatabaseCache(int capacity)
        {
            MaxSize = capacity;
        }

        public void Add(T item)
        {
            if (_set.Count >= MaxSize)
            {
                T toRemove = _list.Last.Value;
                _set.Remove(toRemove);
                _list.RemoveLast();
            }

            _set.Add(item);
            _list.AddFirst(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public bool GetById(string id, out T result)
        {
            result = _set.SingleOrDefault(o => o.Id.Equals(id));
            if (result != default(T))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<T> GetBy(string property, string value)
        {
            return _set.Where(o => 
            {
                return (string)o.GetType().GetProperty(property).GetValue(o) == value;
            });
        }

        public IEnumerator GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        public void Dispose()
        {
            _set.Clear();
            _list.Clear();
        }
    }
}
