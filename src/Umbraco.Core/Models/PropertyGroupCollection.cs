using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Umbraco.Core.Models
{

    /// <summary>
    /// Represents a collection of <see cref="PropertyGroup"/> objects
    /// </summary>
    [Serializable]
    [DataContract]
    // TODO: Change this to ObservableDictionary so we can reduce the INotifyCollectionChanged implementation details
    public class PropertyGroupCollection : KeyedCollection<string, PropertyGroup>, INotifyCollectionChanged, IDeepCloneable
    {
        private readonly ReaderWriterLockSlim _addLocker = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGroupCollection" /> class.
        /// </summary>
        internal PropertyGroupCollection()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGroupCollection" /> class.
        /// </summary>
        /// <param name="groups">The groups.</param>
        public PropertyGroupCollection(IEnumerable<PropertyGroup> groups)
        {
            Reset(groups);
        }

        /// <summary>
        /// Resets the collection to only contain the <see cref="PropertyGroup"/> instances referenced in the <paramref name="groups"/> parameter.
        /// </summary>
        /// <param name="groups">The property groups.</param>
        /// <remarks></remarks>
        internal void Reset(IEnumerable<PropertyGroup> groups)
        {
            // Ccollection events will be raised in each of these calls
            Clear();

            // Collection events will be raised in each of these calls
            foreach (var group in groups)
                Add(group);
        }

        public new PropertyGroup this[string key]
        {
            // TODO Remove this property in v9 (only needed for backwards compatibility with names)
            get
            {
                var index = IndexOfKey(key);
                if (index == -1) throw new KeyNotFoundException();

                return this[index];
            }
        }

        protected override void SetItem(int index, PropertyGroup item)
        {
            var oldItem = index >= 0 ? this[index] : item;

            oldItem.PropertyChanged -= PropertyGroup_PropertyChanged;
            item.PropertyChanged += PropertyGroup_PropertyChanged;

            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
        }

        protected override void RemoveItem(int index)
        {
            var removed = this[index];

            removed.PropertyChanged -= PropertyGroup_PropertyChanged;

            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        protected override void InsertItem(int index, PropertyGroup item)
        {
            item.PropertyChanged += PropertyGroup_PropertyChanged;

            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.PropertyChanged -= PropertyGroup_PropertyChanged;
            }

            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal new void Add(PropertyGroup item)
        {
            try
            {
                _addLocker.EnterWriteLock();

                // Ensure alias is set
                if (string.IsNullOrEmpty(item.Alias))
                {
                    item.Alias = item.Name.ToSafeAlias(true);
                }

                // Note this is done to ensure existing groups can be renamed
                if (item.HasIdentity && item.Id > 0)
                {
                    var index = IndexOfKey(item.Id);
                    if (index != -1)
                    {
                        var keyExists = Contains(item.Alias);
                        if (keyExists)
                            throw new ArgumentException($"Naming conflict: changing the alias of property group '{item.Name}' would result in duplicates.");

                        // Collection events will be raised in SetItem
                        SetItem(index, item);
                        return;
                    }
                }
                else
                {
                    var index = IndexOfKey(item.Alias);
                    if (index != -1)
                    {
                        // Collection events will be raised in SetItem
                        SetItem(index, item);
                        return;
                    }
                }

                // Collection events will be raised in InsertItem
                base.Add(item);
            }
            finally
            {
                if (_addLocker.IsWriteLockHeld)
                    _addLocker.ExitWriteLock();
            }
        }

        private void PropertyGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = sender as PropertyGroup;

            if (e.PropertyName == nameof(PropertyGroup.Alias))
            {
                ChangeItemKey(item, item.Alias);

                // Update child aliases
                foreach (var childItem in this.Where(x => x.GetParentAlias() == item.Alias))
                {
                    childItem.UpdateParentAlias(item.Alias);
                }
            }
        }

        // TODO Remove this method in v9 (only needed for backwards compatibility with names)
        public new bool Remove(string key)
        {
            var index = IndexOfKey(key);
            if (index == -1) return false;

            RemoveAt(index);

            return true;
        }

        // TODO Remove this method in v9 (only needed for backwards compatibility with names)
        public new bool Contains(string groupName) => IndexOfKey(groupName) != -1;

        public bool Contains(int id)
        {
            return this.Any(x => x.Id == id);
        }

        [Obsolete("Use Remove(key) instead.")]
        public void RemoveItem(string propertyGroupName)
        {
            var index = IndexOfKey(propertyGroupName);

            // Only removes an item if the key was found
            if (index != -1)
                RemoveItem(index);
        }

        public int IndexOfKey(string key)
        {
            var index = this.FindIndex(x => x.Alias == key);
            if (index == -1 && !key.Contains('/'))
            {
                // TODO Clean up for v9 (only needed for backwards compatibility with names)
                index = this.FindIndex(x => x.Alias == key.ToSafeAlias(true));
            }

            return index;
        }

        public int IndexOfKey(int id) => this.FindIndex(x => x.Id == id);

        protected override string GetKeyForItem(PropertyGroup item) => item.Alias;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Clears all <see cref="CollectionChanged"/> event handlers
        /// </summary>
        public void ClearCollectionChangedEvents() => CollectionChanged = null;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public object DeepClone()
        {
            var clone = new PropertyGroupCollection();
            foreach (var group in this)
            {
                clone.Add((PropertyGroup)group.DeepClone());
            }

            return clone;
        }
    }
}
