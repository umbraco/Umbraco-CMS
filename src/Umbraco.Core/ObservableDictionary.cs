using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core
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
    public class ObservableDictionary<TKey, TValue> : ObservableCollection<TValue>
    {
        protected Dictionary<TKey, int> Indecies = new Dictionary<TKey, int>();
        protected Func<TValue, TKey> KeySelector;

        /// <summary>
        /// Create new ObservableDictionary
        /// </summary>
        /// <param name="keySelector">Selector function to create key from value</param>
        public ObservableDictionary(Func<TValue, TKey> keySelector)
            : base()
        {
            if (keySelector == null) throw new ArgumentException("keySelector");
            KeySelector = keySelector;
        }

        #region Protected Methods
        protected override void InsertItem(int index, TValue item)
        {
            var key = KeySelector(item);
            if (Indecies.ContainsKey(key))
                throw new DuplicateKeyException(key.ToString());

            if (index != this.Count)
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

        public virtual bool ContainsKey(TKey key)
        {
            return Indecies.ContainsKey(key);
        }

        /// <summary>
        /// Gets or sets the element with the specified key.  If setting a new value, new value must have same key.
        /// </summary>
        /// <param name="key">Key of element to replace</param>
        /// <returns></returns>
        public virtual TValue this[TKey key]
        {

            get { return this[Indecies[key]]; }
            set
            {
                //confirm key matches
                if (!KeySelector(value).Equals(key))
                    throw new InvalidOperationException("Key of new value does not match");

                if (!Indecies.ContainsKey(key))
                {
                    this.Add(value);
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
        public virtual bool Replace(TKey key, TValue value)
        {
            if (!Indecies.ContainsKey(key)) return false;
            //confirm key matches
            if (!KeySelector(value).Equals(key))
                throw new InvalidOperationException("Key of new value does not match");

            this[Indecies[key]] = value;
            return true;

        }

        public virtual bool Remove(TKey key)
        {
            if (!Indecies.ContainsKey(key)) return false;

            this.RemoveAt(Indecies[key]);
            return true;

        }

        /// <summary>
        /// Allows us to change the key of an item
        /// </summary>
        /// <param name="currentKey"></param>
        /// <param name="newKey"></param>
        public virtual void ChangeKey(TKey currentKey, TKey newKey)
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

        internal class DuplicateKeyException : Exception
        {

            public string Key { get; private set; }
            public DuplicateKeyException(string key)
                : base("Attempted to insert duplicate key " + key + " in collection")
            {
                Key = key;
            }
        }

    }
}
