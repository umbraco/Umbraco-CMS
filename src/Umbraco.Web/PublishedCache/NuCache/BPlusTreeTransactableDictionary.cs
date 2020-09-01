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
        public void Commit()
        {
            _bplusTree.Commit();
        }

        public void Rollback()
        {
            _bplusTree.Rollback();
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

        #region IConcurrentDictionary<TKey,TValue>

        public TValue GetOrAdd(TKey key, Converter<TKey, TValue> fnCreate)
        {
            return _bplusTree.GetOrAdd(key, fnCreate);
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, KeyValueUpdate<TKey, TValue> fnUpdate)
        {
            return _bplusTree.AddOrUpdate(key, addValue, fnUpdate);
        }

        public TValue AddOrUpdate(TKey key, Converter<TKey, TValue> fnCreate, KeyValueUpdate<TKey, TValue> fnUpdate)
        {
            return _bplusTree.AddOrUpdate(key, fnCreate, fnUpdate);
        }

        public bool AddOrUpdate<T>(TKey key, ref T createOrUpdateValue) where T : ICreateOrUpdateValue<TKey, TValue>
        {
            return _bplusTree.AddOrUpdate(key, ref createOrUpdateValue);
        }

        public bool TryAdd(TKey key, Converter<TKey, TValue> fnCreate)
        {
            return _bplusTree.TryAdd(key, fnCreate);
        }

        public bool TryUpdate(TKey key, KeyValueUpdate<TKey, TValue> fnUpdate)
        {
            return _bplusTree.TryUpdate(key, fnUpdate);
        }

        public bool TryRemove(TKey key, KeyValuePredicate<TKey, TValue> fnCondition)
        {
            return _bplusTree.TryRemove(key, fnCondition);
        }

        public bool TryRemove<T>(TKey key, ref T removeValue) where T : IRemoveValue<TKey, TValue>
        {
            return _bplusTree.TryRemove(key, ref removeValue);
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            return _bplusTree.GetOrAdd(key, value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return _bplusTree.TryAdd(key, value);
        }

        public bool TryUpdate(TKey key, TValue value)
        {
            return _bplusTree.TryUpdate(key, value);
        }

        public bool TryUpdate(TKey key, TValue value, TValue comparisonValue)
        {
            return _bplusTree.TryUpdate(key, value, comparisonValue);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            return _bplusTree.TryRemove(key, out value);
        }
        #endregion

        #region DictionaryEX

        #endregion
    }
}
