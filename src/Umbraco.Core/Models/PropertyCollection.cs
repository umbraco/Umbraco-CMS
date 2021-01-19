using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{

    /// <summary>
    /// Represents a collection of property values.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class PropertyCollection : KeyedCollection<string, Property>, INotifyCollectionChanged, IDeepCloneable
    {
        private readonly object _addLocker = new object();
        
        internal Func<Property, bool> AdditionValidator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCollection"/> class.
        /// </summary>
        internal PropertyCollection()
            : base(StringComparer.InvariantCultureIgnoreCase)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCollection"/> class.
        /// </summary>
        /// <param name="additionValidator">A function validating added properties.</param>
        internal PropertyCollection(Func<Property, bool> additionValidator)
            : this()
        {
            AdditionValidator = additionValidator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCollection"/> class.
        /// </summary>
        public PropertyCollection(IEnumerable<Property> properties)
            : this()
        {
            Reset(properties);
        }

        /// <summary>
        /// Replaces all properties, whilst maintaining validation delegates.
        /// </summary>
        internal void Reset(IEnumerable<Property> properties)
        {
            //collection events will be raised in each of these calls
            Clear();

            //collection events will be raised in each of these calls
            foreach (var property in properties)
                Add(property);
        }

        /// <summary>
        /// Replaces the property at the specified index with the specified property.
        /// </summary>
        protected override void SetItem(int index, Property property)
        {
            var oldItem = index >= 0 ? this[index] : property;
            base.SetItem(index, property);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, property, oldItem));
        }

        /// <summary>
        /// Removes the property at the specified index.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            var removed = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        /// <summary>
        /// Inserts the specified property at the specified index.
        /// </summary>
        protected override void InsertItem(int index, Property property)
        {
            base.InsertItem(index, property);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, property));
        }

        /// <summary>
        /// Removes all properties.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Adds or updates a property.
        /// </summary>
        internal new void Add(Property property)
        {
            lock (_addLocker) // TODO: why are we locking here and not everywhere else?!
            {
                var key = GetKeyForItem(property);
                if (key != null)
                {
                    if (Contains(key))
                    {
                        // transfer id and values if ...
                        var existing = this[key];

                        if (property.Id == 0 && existing.Id != 0)
                            property.Id = existing.Id;

                        if (property.Values.Count == 0 && existing.Values.Count > 0)
                            property.Values = existing.Values.Select(x => x.Clone()).ToList();

                        // replace existing with property and return,
                        // SetItem invokes OnCollectionChanged (but not OnAdd)
                        SetItem(IndexOfKey(key), property);
                        return;
                    }
                }

                //collection events will be raised in InsertItem with Add
                base.Add(property);
            }
        }

        /// <summary>
        /// Gets the index for a specified property alias.
        /// </summary>
        public int IndexOfKey(string key)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Alias.InvariantEquals(key))
                    return i;
            }
            return -1;
        }

        protected override string GetKeyForItem(Property item)
        {
            return item.Alias;
        }

        /// <summary>
        /// Gets the property with the specified PropertyType.
        /// </summary>
        internal Property this[PropertyType propertyType]
        {
            get
            {
                return this.FirstOrDefault(x => x.Alias.InvariantEquals(propertyType.Alias));
            }
        }

        public bool TryGetValue(string propertyTypeAlias, out Property property)
        {
            property = this.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            return property != null;
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void ClearCollectionChangedEvents() => CollectionChanged = null;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Ensures that the collection contains properties for the specified property types.
        /// </summary>
        protected internal void EnsurePropertyTypes(IEnumerable<PropertyType> propertyTypes)
        {
            if (propertyTypes == null)
                return;

            foreach (var propertyType in propertyTypes)
                Add(new Property(propertyType));
        }

        /// <summary>
        /// Ensures that the collection does not contain properties not in the specified property types.
        /// </summary>
        protected internal void EnsureCleanPropertyTypes(IEnumerable<PropertyType> propertyTypes)
        {
            if (propertyTypes == null)
                return;

            var propertyTypesA = propertyTypes.ToArray();

            var thisAliases = this.Select(x => x.Alias);
            var typeAliases = propertyTypesA.Select(x => x.Alias);
            var remove = thisAliases.Except(typeAliases).ToArray();
            foreach (var alias in remove)
                Remove(alias);

            foreach (var propertyType in propertyTypesA)
                Add(new Property(propertyType));
        }

        /// <summary>
        /// Deep clones.
        /// </summary>
        public object DeepClone()
        {
            var clone = new PropertyCollection();
            foreach (var property in this)
                clone.Add((Property) property.DeepClone());
            return clone;
        }
    }
}
