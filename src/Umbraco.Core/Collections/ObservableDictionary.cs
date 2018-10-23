using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// An ObservableDictionary
    /// </summary>
    /// <remarks>
    /// Assumes that the key will not change and is unique for each element in the collection.
    /// Collection is not thread-safe, so calls should be made single-threaded.
    /// </remarks>
    /// <typeparam name="TValue">The type of elements contained in the BindableCollection</typeparam>
    /// <typeparam name="TKey">The type of the indexing key</typeparam>
    public class ObservableDictionary<TKey, TValue> : ObservableCollection<TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {
        protected Dictionary<TKey, int> Indecies { get; }
        protected Func<TValue, TKey> KeySelector { get; }

        /// <summary>
        /// Create new ObservableDictionary
        /// </summary>
        /// <param name="keySelector">Selector function to create key from value</param>
        /// <param name="equalityComparer">The equality comparer to use when comparing keys, or null to use the default comparer.</param>
        public ObservableDictionary(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> equalityComparer = null)
        {
            KeySelector = keySelector ?? throw new ArgumentException("keySelector");
            Indecies = new Dictionary<TKey, int>(equalityComparer);
        }

        #region Protected Methods

        protected override void InsertItem(int index, TValue item)
        {
            var key = KeySelector(item);
            if (Indecies.ContainsKey(key))
                throw new DuplicateKeyException(key.ToString());

            if (index != Count)
            {
                foreach (var k in Indecies.Keys.Where(k => Indecies[k] >= index).ToList())
                {
                    Indecies[k]++;
                }
            }

            base.InsertItem(index, item);
            Indecies[key] = index;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            Indecies.Clear();
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            var key = KeySelector(item);

            base.RemoveItem(index);

            Indecies.Remove(key);

            foreach (var k in Indecies.Keys.Where(k => Indecies[k] > index).ToList())
            {
                Indecies[k]--;
            }
        }

        #endregion

        public bool ContainsKey(TKey key)
        {
            return Indecies.ContainsKey(key);
        }

        /// <summary>
        /// Gets or sets the element with the specified key.  If setting a new value, new value must have same key.
        /// </summary>
        /// <param name="key">Key of element to replace</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {

            get => this[Indecies[key]];
            set
            {
                //confirm key matches
                if (!KeySelector(value).Equals(key))
                    throw new InvalidOperationException("Key of new value does not match");

                if (!Indecies.ContainsKey(key))
                {
                    Add(value);
                }
                else
                {
                    this[Indecies[key]] = value;
                }
            }
        }

        /// <summary>
        /// Replaces element at given key with new value.  New value must have same key.
        /// </summary>
        /// <param name="key">Key of element to replace</param>
        /// <param name="value">New value</param>
        ///
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>False if key not found</returns>
        public bool Replace(TKey key, TValue value)
        {
            if (!Indecies.ContainsKey(key)) return false;

            //confirm key matches
            if (!KeySelector(value).Equals(key))
                throw new InvalidOperationException("Key of new value does not match");

            this[Indecies[key]] = value;
            return true;

        }

        public bool Remove(TKey key)
        {
            if (!Indecies.ContainsKey(key)) return false;

            RemoveAt(Indecies[key]);
            return true;

        }

        /// <summary>
        /// Allows us to change the key of an item
        /// </summary>
        /// <param name="currentKey"></param>
        /// <param name="newKey"></param>
        public void ChangeKey(TKey currentKey, TKey newKey)
        {
            if (!Indecies.ContainsKey(currentKey))
            {
                throw new InvalidOperationException("No item with the key " + currentKey + "was found in the collection");
            }

            if (ContainsKey(newKey))
            {
                throw new DuplicateKeyException(newKey.ToString());
            }

            var currentIndex = Indecies[currentKey];

            Indecies.Remove(currentKey);
            Indecies.Add(newKey, currentIndex);
        }

        #region IDictionary and IReadOnlyDictionary implementation

        public bool TryGetValue(TKey key, out TValue val)
        {
            if (Indecies.TryGetValue(key, out var index))
            {
                val = this[index];
                return true;
            }
            val = default;
            return false;
        }

        /// <summary>
        /// Returns all keys
        /// </summary>
        public IEnumerable<TKey> Keys => Indecies.Keys;

        /// <summary>
        /// Returns all values
        /// </summary>
        public IEnumerable<TValue> Values => base.Items;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Indecies.Keys;

        //this will never be used
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToList();

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var i in Values)
            {
                var key = KeySelector(i);
                yield return new KeyValuePair<TKey, TValue>(key, i);
            }
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            Add(value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        internal class DuplicateKeyException : Exception
        {
            public DuplicateKeyException(string key)
                : base("Attempted to insert duplicate key \"" + key + "\" in collection.")
            {
                Key = key;
            }

            public string Key { get; }
        }
    }
}
