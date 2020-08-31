using CSharpTest.Net.Collections;
using CSharpTest.Net.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class BPlusTreeTransactableDictionary<TKey, TValue> : ITransactableDictionary<TKey, TValue>
    {
        private readonly BPlusTree<TKey, TValue> _bplusTree;
        private bool _disposedValue;

        public BPlusTreeTransactableDictionary(BPlusTree<TKey, TValue> bplusTree)
        {
            _bplusTree = bplusTree;
        }

        public TValue this[TKey key] { get => _bplusTree[key]; set => _bplusTree[key] = value; }

        public ICollection<TKey> Keys => _bplusTree.Keys;

        public ICollection<TValue> Values => _bplusTree.Values;

        public int Count => _bplusTree.Count;

        public bool IsReadOnly => _bplusTree.IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            _bplusTree.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _bplusTree.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _bplusTree.Clear();
        }

        public void Commit()
        {
            _bplusTree.Commit();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _bplusTree.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _bplusTree.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _bplusTree.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _bplusTree.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return _bplusTree.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item);
        }

        public void Rollback()
        {
            _bplusTree.Rollback();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _bplusTree.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _bplusTree.GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _bplusTree.Dispose();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void TryRemove(TKey key, out TValue unused)
        {
            _bplusTree.TryRemove(key, out unused);
        }
    }
}
