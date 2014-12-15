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
    /// Represents a collection of <see cref="PropertyGroup"/> objects
    /// </summary>
    [Serializable]
    [DataContract]
    public class PropertyGroupCollection : KeyedCollection<string, PropertyGroup>, INotifyCollectionChanged, IDeepCloneable
    {
        private readonly ReaderWriterLockSlim _addLocker = new ReaderWriterLockSlim();
        
        internal Action OnAdd;

        internal PropertyGroupCollection()
        {
            
        }

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
            Clear();
            groups.ForEach(Add);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void SetItem(int index, PropertyGroup item)
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
            using (new WriteLock(_addLocker))
            {
                //Note this is done to ensure existig groups can be renamed
                if (item.HasIdentity && item.Id > 0)
                {
                    var exists = this.Contains(item.Id);
                    if (exists)
                    {
                        var keyExists = this.Contains(item.Name);
                        if(keyExists)
                            throw new Exception(string.Format("Naming conflict: Changing the name of PropertyGroup '{0}' would result in duplicates", item.Name));

                        SetItem(IndexOfKey(item.Id), item);
                        return;
                    }
                }
                else
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
                }
                
                base.Add(item);
                OnAdd.IfNotNull(x => x.Invoke());//Could this not be replaced by a Mandate/Contract for ensuring item is not null

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
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
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Name == key)
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOfKey(int id)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Id == id)
                {
                    return i;
                }
            }
            return -1;
        }

        protected override string GetKeyForItem(PropertyGroup item)
        {
            return item.Name;
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
            var newGroup = new PropertyGroupCollection();
            foreach (var p in this)
            {
                newGroup.Add((PropertyGroup)p.DeepClone());
            }            
            return newGroup;
        }
    }
}