using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a collection of <see cref="PropertyType"/> objects
    /// </summary>
    [Serializable]
    [DataContract]
    public class PropertyTypeCollection : KeyedCollection<string, PropertyType>, INotifyCollectionChanged, IDeepCloneable
    {
        [IgnoreDataMember]
        private readonly ReaderWriterLockSlim _addLocker = new ReaderWriterLockSlim();

        [IgnoreDataMember]
        internal Action OnAdd;

        internal PropertyTypeCollection()
        {
            
        }

        public PropertyTypeCollection(IEnumerable<PropertyType> properties)
        {
            Reset(properties);
        }

        /// <summary>
        /// Resets the collection to only contain the <see cref="PropertyType"/> instances referenced in the <paramref name="properties"/> parameter.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <remarks></remarks>
        internal void Reset(IEnumerable<PropertyType> properties)
        {
            Clear();
            properties.ForEach(Add);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void SetItem(int index, PropertyType item)
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

        protected override void InsertItem(int index, PropertyType item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal new void Add(PropertyType item)
        {
            using (new WriteLock(_addLocker))
            {
                var key = GetKeyForItem(item);
                if (key != null)
                {
                    var exists = this.Contains(key);
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
                OnAdd.IfNotNull(x => x.Invoke());//Could this not be replaced by a Mandate/Contract for ensuring item is not null

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
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

        public void RemoveItem(string propertyTypeAlias)
        {
            var key = IndexOfKey(propertyTypeAlias);
            //Only removes an item if the key was found
            if(key != -1)
                RemoveItem(key);
        }

        public int IndexOfKey(string key)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Alias == key)
                {
                    return i;
                }
            }
            return -1;
        }

        protected override string GetKeyForItem(PropertyType item)
        {
            return item.Alias;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }

        public object DeepClone()
        {
            var newGroup = new PropertyTypeCollection();
            foreach (var p in this)
            {
                newGroup.Add((PropertyType)p.DeepClone());
            }
            return newGroup;
        }
    }
}