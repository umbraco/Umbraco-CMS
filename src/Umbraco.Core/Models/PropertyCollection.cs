using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models
{

    /// <summary>
    /// Represents a collection of property values.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class PropertyCollection : KeyedCollection<string, IProperty>, IPropertyCollection
    {
        private readonly object _addLocker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCollection"/> class.
        /// </summary>
        public PropertyCollection()
            : base(StringComparer.InvariantCultureIgnoreCase)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCollection"/> class.
        /// </summary>
        public PropertyCollection(IEnumerable<IProperty> properties)
            : this()
        {
            Reset(properties);
        }

        /// <summary>
        /// Replaces all properties, whilst maintaining validation delegates.
        /// </summary>
        private void Reset(IEnumerable<IProperty> properties)
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
        protected override void SetItem(int index, IProperty property)
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
        protected override void InsertItem(int index, IProperty property)
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

        /// <inheritdoc />
        public new void Add(IProperty property)
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
        private int IndexOfKey(string key)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Alias?.InvariantEquals(key) ?? false)
                    return i;
            }
            return -1;
        }

        protected override string GetKeyForItem(IProperty item)
        {
            return item.Alias!;
        }

        /// <summary>
        /// Gets the property with the specified PropertyType.
        /// </summary>
        internal IProperty? this[IPropertyType propertyType]
        {
            get
            {
                return this.FirstOrDefault(x => x.Alias.InvariantEquals(propertyType.Alias));
            }
        }

        public bool TryGetValue(string propertyTypeAlias, [MaybeNullWhen(false)] out IProperty property)
        {
            property = this.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            return property != null;
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void ClearCollectionChangedEvents() => CollectionChanged = null;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }


        /// <inheritdoc />
        public void EnsurePropertyTypes(IEnumerable<IPropertyType> propertyTypes)
        {
            if (propertyTypes == null)
                return;

            foreach (var propertyType in propertyTypes)
                Add(new Property(propertyType));
        }


        /// <inheritdoc />
        public void EnsureCleanPropertyTypes(IEnumerable<IPropertyType> propertyTypes)
        {
            if (propertyTypes == null)
                return;

            var propertyTypesA = propertyTypes.ToArray();

            var thisAliases = this.Select(x => x.Alias);
            var typeAliases = propertyTypesA.Select(x => x.Alias);
            var remove = thisAliases.Except(typeAliases).ToArray();
            foreach (var alias in remove)
            {
                if (alias is not null)
                {
                    Remove(alias);
                }

            }


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
