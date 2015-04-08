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
    /// Represents a Collection of <see cref="Property"/> objects
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class PropertyCollection : KeyedCollection<string, Property>, INotifyCollectionChanged, IDeepCloneable
    {
        private readonly object _addLocker = new object();
        internal Action OnAdd;
        internal Func<Property, bool> ValidateAdd { get; set; }

        internal PropertyCollection()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCollection"/> class with a delegate responsible for validating the addition of <see cref="Property"/> instances.
        /// </summary>
        /// <param name="validationCallback">The validation callback.</param>
        /// <remarks></remarks>
        internal PropertyCollection(Func<Property, bool> validationCallback)
            : this()
        {
            ValidateAdd = validationCallback;
        }

        public PropertyCollection(IEnumerable<Property> properties)
            : this()
        {
            Reset(properties);
        }

        /// <summary>
        /// Resets the collection to only contain the <see cref="Property"/> instances referenced in the <paramref name="properties"/> parameter, whilst maintaining
        /// any validation delegates such as <see cref="ValidateAdd"/>
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <remarks></remarks>
        internal void Reset(IEnumerable<Property> properties)
        {
            Clear();
            properties.ForEach(Add);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void SetItem(int index, Property item)
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

        protected override void InsertItem(int index, Property item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal new void Add(Property item)
        {
            lock (_addLocker)
            {
                var key = GetKeyForItem(item);
                if (key != null)
                {
                    var exists = this.Contains(key);
                    if (exists)
                    {
                        //NOTE: Consider checking type before value is set: item.PropertyType.DataTypeId == property.PropertyType.DataTypeId
                        //Transfer the existing value to the new property
                        var property = this[key];
                        if (item.Value == null && property.Value != null)
                        {
                            item.Value = property.Value;
                        }

                        SetItem(IndexOfKey(key), item);
                        return;
                    }
                }
                base.Add(item);
                OnAdd.IfNotNull(x => x.Invoke());//Could this not be replaced by a Mandate/Contract for ensuring item is not null

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="Property"/> whose alias matches the specified PropertyType.
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType.</param>
        /// <returns><c>true</c> if the collection contains the specified alias; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public new bool Contains(string propertyTypeAlias)
        {
            return base.Contains(propertyTypeAlias);
        }

        public int IndexOfKey(string key)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Alias.InvariantEquals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        protected override string GetKeyForItem(Property item)
        {
            return item.Alias;
        }

        /// <summary>
        /// Gets the element with the specified PropertyType.
        /// </summary>
        /// 
        /// <returns>
        /// The element with the specified PropertyType. If an element with the specified PropertyType is not found, an exception is thrown.
        /// </returns>
        /// <param name="propertyType">The PropertyType of the element to get.</param><exception cref="T:System.ArgumentNullException"><paramref name="propertyType"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">An element with the specified key does not exist in the collection.</exception>
        internal Property this[PropertyType propertyType]
        {
            get
            {
                return this.FirstOrDefault(x => x.Alias.InvariantEquals(propertyType.Alias));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }

        /// <summary>
        /// Ensures that the collection contains Properties for the passed in PropertyTypes
        /// </summary>
        /// <param name="propertyTypes">List of PropertyType</param>
        protected internal void EnsurePropertyTypes(IEnumerable<PropertyType> propertyTypes)
        {
            if (/*!this.Any() &&*/ propertyTypes != null)
            {
                foreach (var propertyType in propertyTypes)
                {
                    Add(new Property(propertyType));
                }
            }
        }

        /// <summary>
        /// Ensures that the collection is cleared from PropertyTypes not in the list of passed in PropertyTypes
        /// </summary>
        /// <param name="propertyTypes">List of PropertyType</param>
        protected internal void EnsureCleanPropertyTypes(IEnumerable<PropertyType> propertyTypes)
        {
            if (propertyTypes != null)
            {
                //Remove PropertyTypes that doesn't exist in the list of new PropertyTypes
                var aliases = this.Select(p => p.Alias).Except(propertyTypes.Select(x => x.Alias)).ToList();
                foreach (var alias in aliases)
                {
                    Remove(alias);
                }

                //Add new PropertyTypes from the list of passed in PropertyTypes
                foreach (var propertyType in propertyTypes)
                {
                    Add(new Property(propertyType));
                }
            }
        }

        /// <summary>
        /// Create a deep clone of this property collection
        /// </summary>
        /// <returns></returns>
        public object DeepClone()
        {
            var newList = new PropertyCollection();
            foreach (var p in this)
            {
                newList.Add((Property)p.DeepClone());
            }
            return newList;
        }
    }
}