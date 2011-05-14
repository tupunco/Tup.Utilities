using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Tup.Utilities
{
    /// <summary>
    /// A simple hashset, built on Dictionary{K, V}
    /// </summary>
    /// <remarks>FROM MS DLR</remarks>
    [Serializable]
    public sealed class HashSet<T> : ICollection<T>
    {
        private readonly Dictionary<T, object> _data;

        public HashSet()
        {
            _data = new Dictionary<T, object>();
        }

        public HashSet(IEqualityComparer<T> comparer)
        {
            _data = new Dictionary<T, object>(comparer);
        }

        public HashSet(IList<T> list)
        {
            _data = new Dictionary<T, object>(list.Count);
            foreach (T t in list)
            {
                //_data.Add(t, null);
                _data[t] = null;
            }
        }

        public HashSet(ICollection<T> list)
        {
            _data = new Dictionary<T, object>(list.Count);
            foreach (T t in list)
            {
                //_data.Add(t, null);
                _data[t] = null;
            }
        }

        public void Add(T item)
        {
            _data[item] = null;
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(T item)
        {
            return _data.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _data.Keys.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return _data.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.Keys.GetEnumerator();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T t in other)
            {
                Add(t);
            }
        }
        /// <summary>
        /// ToList
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return new List<T>(_data.Keys);
        }
    }
}
