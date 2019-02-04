using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a collection of <see cref="PropertyType"/> objects.
    /// </summary>
    [Serializable]
    [DataContract]
    // TODO: Change this to ObservableDictionary so we can reduce the INotifyCollectionChanged implementation details
    public class PropertyTypeCollection : KeyedCollection<string, PropertyType>, INotifyCollectionChanged, IDeepCloneable
    {
        [IgnoreDataMember]
        private readonly ReaderWriterLockSlim _addLocker = new ReaderWriterLockSlim();

        // TODO: This doesn't seem to be used
        [IgnoreDataMember]
        internal Action OnAdd;

        internal PropertyTypeCollection(bool isPublishing)
        {
            IsPublishing = isPublishing;
        }

        public PropertyTypeCollection(bool isPublishing, IEnumerable<PropertyType> properties)
            : this(isPublishing)
        {
            Reset(properties);
        }

        public bool IsPublishing { get; }

        /// <summary>
        /// Resets the collection to only contain the <see cref="PropertyType"/> instances referenced in the <paramref name="properties"/> parameter.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <remarks></remarks>
        internal void Reset(IEnumerable<PropertyType> properties)
        {
            Clear();
            foreach (var property in properties)
                Add(property);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void SetItem(int index, PropertyType item)
        {
            item.IsPublishing = IsPublishing;
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var removed = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        protected override void InsertItem(int index, PropertyType item)
        {
            item.IsPublishing = IsPublishing;
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        // TODO: Instead of 'new' this should explicitly implement one of the collection interfaces members
        internal new void Add(PropertyType item)
        {
            item.IsPublishing = IsPublishing;

            // TODO: this is not pretty and should be refactored
            try
            {
                _addLocker.EnterWriteLock();
                var key = GetKeyForItem(item);
                if (key != null)
                {
                    var exists = Contains(key);
                    if (exists)
                    {
                        SetItem(IndexOfKey(key), item);
                        return;
                    }
                }

                //check if the item's sort order is already in use
                if (this.Any(x => x.SortOrder == item.SortOrder))
                {
                    //make it the next iteration
                    item.SortOrder = this.Max(x => x.SortOrder) + 1;
                }

                base.Add(item);
                OnAdd?.Invoke();

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            finally
            {
                if (_addLocker.IsWriteLockHeld)
                    _addLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="Property"/> whose alias matches the specified PropertyType.
        /// </summary>
        /// <param name="propertyAlias">Alias of the PropertyType.</param>
        /// <returns><c>true</c> if the collection contains the specified alias; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public new bool Contains(string propertyAlias)
        {
            return this.Any(x => x.Alias == propertyAlias);
        }

        public bool RemoveItem(string propertyTypeAlias)
        {
            var key = IndexOfKey(propertyTypeAlias);
            if (key != -1) RemoveItem(key);
            return key != -1;
        }

        public int IndexOfKey(string key)
        {
            for (var i = 0; i < Count; i++)
                if (this[i].Alias == key)
                    return i;
            return -1;
        }

        protected override string GetKeyForItem(PropertyType item)
        {
            return item.Alias;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public object DeepClone()
        {
            var clone = new PropertyTypeCollection(IsPublishing);
            foreach (var propertyType in this)
                clone.Add((PropertyType) propertyType.DeepClone());
            return clone;
        }
    }
}
