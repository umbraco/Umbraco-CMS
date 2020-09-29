using CSharpTest.Net.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class BPlusTreeTransactableDictionary<TKey, TValue> : ITransactableDictionary<TKey, TValue>
    {
        private readonly BPlusTree<TKey, TValue> _bplusTree;
        private bool _disposedValue;
        private readonly string _filePath;
        private bool _isPopulated;

        public BPlusTreeTransactableDictionary(BPlusTree<TKey, TValue> bplusTree, string filePath,bool localDbCacheFileExists)
        {
            _bplusTree = bplusTree;
            _filePath = filePath;
            _isPopulated = localDbCacheFileExists;
        }

        #region IDictionary
        public TValue this[TKey key] { get => _bplusTree[key]; set => _bplusTree[key] = value; }

        public ICollection<TKey> Keys => _bplusTree.Keys;

        public ICollection<TValue> Values => _bplusTree.Values;
        public void Add(TKey key, TValue value)
        {
            _bplusTree.Add(key, value);
        }
        public bool ContainsKey(TKey key)
        {
            return _bplusTree.ContainsKey(key);
        }
        public bool Remove(TKey key)
        {
            return _bplusTree.Remove(key);
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _bplusTree.TryGetValue(key, out value);
        }
        #endregion

        #region ICollection<T>
        public int Count => _bplusTree.Count;

        public bool IsReadOnly => _bplusTree.IsReadOnly;

        public void Clear()
        {
            _bplusTree.Clear();
        }
        #endregion

        #region IEnumerable<T>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _bplusTree.GetEnumerator();
        }
        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _bplusTree.Add(item.Key, item.Value);
        }
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _bplusTree.Contains(item);
        }
       
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _bplusTree.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item);
        }
        #endregion

        #region ITransactable
        public ITransactionScope BeginTransaction()
        {
            return new BPlusTreeTransactionScope<TKey,TValue>(_bplusTree);
        }

        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _bplusTree.GetEnumerator();
        }
        #endregion

        #region IDisposable
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

        #endregion

        #region ITransactableDictionary<TKey,TValue>

        public bool TryRemove(TKey key, out TValue value)
        {
            return _bplusTree.TryRemove(key, out value);
        }
        #endregion

        public void Drop()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        public bool IsPopulated() => _isPopulated;
    }
}
