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
    /// Represents a collection of <see cref="PropertyGroup"/> objects
    /// </summary>
    [Serializable]
    [DataContract]
    // TODO: Change this to ObservableDictionary so we can reduce the INotifyCollectionChanged implementation details
    public class PropertyGroupCollection : KeyedCollection<string, PropertyGroup>, INotifyCollectionChanged, IDeepCloneable
    {
        private readonly ReaderWriterLockSlim _addLocker = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGroupCollection"/> class.
        /// </summary>
        /// <remarks>
        /// Property group names are case-insensitive and the internal lookup dictionary is disabled to support renaming groups.
        /// </remarks>
        internal PropertyGroupCollection()
            : base(StringComparer.InvariantCultureIgnoreCase, -1)
        { }

        public PropertyGroupCollection(IEnumerable<PropertyGroup> groups)
            : this()
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

        protected override void SetItem(int index, PropertyGroup item)
        {
            var oldItem = index >= 0 ? this[index] : item;
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
        }

        protected override void RemoveItem(int index)
        {
            var removed = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        protected override void InsertItem(int index, PropertyGroup item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal new void Add(PropertyGroup item)
        {
            try
            {
                _addLocker.EnterWriteLock();

                // Note this is done to ensure existing groups can be renamed
                if (item.HasIdentity && item.Id > 0)
                {
                    var index = IndexOfKey(item.Id);
                    if (index != -1)
                    {
                        var keyExists = Contains(item.Name);
                        if (keyExists)
                            throw new Exception($"Naming conflict: Changing the name of PropertyGroup '{item.Name}' would result in duplicates");

                        // Collection events will be raised in SetItem
                        SetItem(index, item);
                        return;
                    }
                }
                else
                {
                    var index = IndexOfKey(item.Name);
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

        public bool Contains(int id)
        {
            return this.Any(x => x.Id == id);
        }

        public void RemoveItem(string propertyGroupName)
        {
            var index = IndexOfKey(propertyGroupName);

            // Only removes an item if the key was found
            if (index != -1)
                RemoveItem(index);
        }

        public int IndexOfKey(string key) => this.FindIndex(x => x.Name.InvariantEquals(key));

        public int IndexOfKey(int id) => this.FindIndex(x => x.Id == id);

        protected override string GetKeyForItem(PropertyGroup item) => item.Name;

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
