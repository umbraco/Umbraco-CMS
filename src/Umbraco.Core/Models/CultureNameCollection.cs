using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Umbraco.Core.Collections;

namespace Umbraco.Core.Models
{
    

    /// <summary>
    /// The culture names of a content's variants
    /// </summary>
    public class CultureNameCollection : KeyedCollection<string, CultureName>, INotifyCollectionChanged, IDeepCloneable, IReadOnlyKeyedCollection<string, CultureName>
    {
        /// <summary>
        /// Creates a new collection from another collection
        /// </summary>
        /// <param name="names"></param>
        public CultureNameCollection(IEnumerable<CultureName> names) : base(StringComparer.InvariantCultureIgnoreCase)
        {
            foreach (var n in names)
                Add(n);
        }

        /// <summary>
        /// Creates a new collection
        /// </summary>
        public CultureNameCollection() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Returns all keys in the collection
        /// </summary>
        public IEnumerable<string> Keys => Dictionary != null ? Dictionary.Keys : this.Select(x => x.Culture);

        public bool TryGetValue(string culture, out CultureName name)
        {
            name = this.FirstOrDefault(x => x.Culture.InvariantEquals(culture));
            return name != null;
        }

        /// <summary>
        /// Add or update the <see cref="CultureName"/>
        /// </summary>
        /// <param name="name"></param>
        public void AddOrUpdate(string culture, string name, DateTime date)
        {
            culture = culture.ToLowerInvariant();
            if (TryGetValue(culture, out var found))
            {
                found.Name = name;
                found.Date = date;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, found, found));
            }
            else
                Add(new CultureName(culture)
                {
                    Name = name,
                    Date = date
                });    
        }

        /// <summary>
        /// Gets the index for a specified culture
        /// </summary>
        public int IndexOfKey(string key)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Culture.InvariantEquals(key))
                    return i;
            }
            return -1;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public object DeepClone()
        {
            var clone = new CultureNameCollection();
            foreach (var name in this)
            {
                clone.Add((CultureName)name.DeepClone());
            }
            return clone;
        }

        protected override string GetKeyForItem(CultureName item)
        {
            return item.Culture;
        }

        /// <summary>
        /// Resets the collection to only contain the <see cref="CultureName"/> instances referenced in the <paramref name="names"/> parameter.
        /// </summary>
        /// <param name="names">The property groups.</param>
        /// <remarks></remarks>
        internal void Reset(IEnumerable<CultureName> names)
        {
            Clear();
            foreach (var name in names)
                Add(name);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void SetItem(int index, CultureName item)
        {
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var removed = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        protected override void InsertItem(int index, CultureName item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        
    }
}
