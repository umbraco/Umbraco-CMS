﻿using System;
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

        internal PropertyGroupCollection()
        { }

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
            //collection events will be raised in each of these calls
            Clear();

            //collection events will be raised in each of these calls
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

                //Note this is done to ensure existing groups can be renamed
                if (item.HasIdentity && item.Id > 0)
                {
                    var exists = Contains(item.Id);
                    if (exists)
                    {
                        var keyExists = Contains(item.Name);
                        if (keyExists)
                            throw new Exception($"Naming conflict: Changing the name of PropertyGroup '{item.Name}' would result in duplicates");

                        //collection events will be raised in SetItem
                        SetItem(IndexOfKey(item.Id), item);
                        return;
                    }
                }
                else
                {
                    var key = GetKeyForItem(item);
                    if (key != null)
                    {
                        var exists = Contains(key);
                        if (exists)
                        {
                            //collection events will be raised in SetItem
                            SetItem(IndexOfKey(key), item);
                            return;
                        }
                    }
                }
                //collection events will be raised in InsertItem
                base.Add(item);
            }
            finally
            {
                if (_addLocker.IsWriteLockHeld)
                    _addLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="PropertyGroup"/> whose name matches the specified parameter.
        /// </summary>
        /// <param name="groupName">Name of the PropertyGroup.</param>
        /// <returns><c>true</c> if the collection contains the specified name; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public new bool Contains(string groupName)
        {
            return this.Any(x => x.Name == groupName);
        }

        public bool Contains(int id)
        {
            return this.Any(x => x.Id == id);
        }

        public void RemoveItem(string propertyGroupName)
        {
            var key = IndexOfKey(propertyGroupName);
            //Only removes an item if the key was found
            if (key != -1)
                RemoveItem(key);
        }

        public int IndexOfKey(string key)
        {
            for (var i = 0; i < Count; i++)
                if (this[i].Name == key)
                    return i;
            return -1;
        }

        public int IndexOfKey(int id)
        {
            for (var i = 0; i < Count; i++)
                if (this[i].Id == id)
                    return i;
            return -1;
        }

        protected override string GetKeyForItem(PropertyGroup item)
        {
            return item.Name;
        }

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
