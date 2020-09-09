using System;
using System.Collections;
using System.Collections.Generic;

namespace ChefKnivesCommentsDatabase.Utility
{
    /// <summary>
    /// Cache of the most recent items added
    /// </summary>
    public class DatabaseCache<T> : IEnumerable, IDisposable
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
            if(_set.Count >= MaxSize)
            {
                T toRemove = _list.Last.Value;
                _set.Remove(toRemove);
                _list.RemoveLast();
            }

            _set.Add(item);
            _list.AddFirst(item);
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
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
