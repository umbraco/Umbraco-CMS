﻿using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A group of property types, which corresponds to the properties grouped under a Tab.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}")]
    public class PropertyGroup : EntityBase, IEquatable<PropertyGroup>
    {
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private string _name;
        private int _sortOrder;
        private PropertyTypeCollection _propertyTypes;

        public PropertyGroup(bool isPublishing)
            : this(new PropertyTypeCollection(isPublishing))
        { }

        public PropertyGroup(PropertyTypeCollection propertyTypeCollection)
        {
            PropertyTypes = propertyTypeCollection;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, string>(x => x.Name);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, int>(x => x.SortOrder);
            public readonly PropertyInfo PropertyTypeCollectionSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, PropertyTypeCollection>(x => x.PropertyTypes);
        }

        private void PropertyTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyTypeCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Name of the Group, which corresponds to the Tab-name in the UI
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector);
        }

        /// <summary>
        /// Gets or sets the Sort Order of the Group
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector);
        }

        /// <summary>
        /// Gets or sets a collection of PropertyTypes for this PropertyGroup
        /// </summary>
        [DataMember]
        public PropertyTypeCollection PropertyTypes
        {
            get => _propertyTypes;
            set
            {
                _propertyTypes = value;

                // since we're adding this collection to this group,
                // we need to ensure that all the lazy values are set.
                foreach (var propertyType in _propertyTypes)
                    propertyType.PropertyGroupId = new Lazy<int>(() => Id);

                _propertyTypes.CollectionChanged += PropertyTypesChanged;
            }
        }

        public bool Equals(PropertyGroup other)
        {
            if (base.Equals(other)) return true;
            return other != null && Name.InvariantEquals(other.Name);
        }

        public override int GetHashCode()
        {
            var baseHash = base.GetHashCode();
            var nameHash = Name.ToLowerInvariant().GetHashCode();
            return baseHash ^ nameHash;
        }
    }
}
