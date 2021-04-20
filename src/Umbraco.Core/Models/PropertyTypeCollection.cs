using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

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
        internal PropertyTypeCollection(bool supportsPublishing)
        {
            SupportsPublishing = supportsPublishing;
        }

        public PropertyTypeCollection(bool supportsPublishing, IEnumerable<PropertyType> properties)
            : this(supportsPublishing)
        {
            Reset(properties);
        }

        public bool SupportsPublishing { get; }

        /// <summary>
        /// Resets the collection to only contain the <see cref="PropertyType"/> instances referenced in the <paramref name="properties"/> parameter.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <remarks></remarks>
        internal void Reset(IEnumerable<PropertyType> properties)
        {
            //collection events will be raised in each of these calls
            Clear();

            //collection events will be raised in each of these calls
            foreach (var property in properties)
                Add(property);            
        }

        protected override void SetItem(int index, PropertyType item)
        {
            item.SupportsPublishing = SupportsPublishing;
            var oldItem = index >= 0 ? this[index] : item;
            base.SetItem(index, item);            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
            item.PropertyChanged += Item_PropertyChanged;
        }

        protected override void RemoveItem(int index)
        {
            var removed = this[index];
            base.RemoveItem(index);
            removed.PropertyChanged -= Item_PropertyChanged;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        protected override void InsertItem(int index, PropertyType item)
        {
            item.SupportsPublishing = SupportsPublishing;
            base.InsertItem(index, item);            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            item.PropertyChanged += Item_PropertyChanged;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            foreach (var item in this)
                item.PropertyChanged -= Item_PropertyChanged;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        // TODO: Instead of 'new' this should explicitly implement one of the collection interfaces members
        internal new void Add(PropertyType item)
        {
            item.SupportsPublishing = SupportsPublishing;

            // TODO: this is not pretty and should be refactored

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

            //check if the item's sort order is already in use
            if (this.Any(x => x.SortOrder == item.SortOrder))
            {
                //make it the next iteration
                item.SortOrder = this.Max(x => x.SortOrder) + 1;
            }

            //collection events will be raised in InsertItem
            base.Add(item);
        }

        /// <summary>
        /// Occurs when a property changes on a PropertyType that exists in this collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var propType = (PropertyType)sender;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, propType, propType));
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
            var clone = new PropertyTypeCollection(SupportsPublishing);
            foreach (var propertyType in this)
                clone.Add((PropertyType) propertyType.DeepClone());
            return clone;
        }
    }
}
