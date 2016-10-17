﻿using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A group of property types, which corresponds to the properties grouped under a Tab.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}")]
    public class PropertyGroup : Entity, IEquatable<PropertyGroup>
    {
        private string _name;
        private int _sortOrder;
        private PropertyTypeCollection _propertyTypes;

        public PropertyGroup() : this(new PropertyTypeCollection())
        {
        }

        public PropertyGroup(PropertyTypeCollection propertyTypeCollection)
        {
            PropertyTypes = propertyTypeCollection;
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, string>(x => x.Name);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, int>(x => x.SortOrder);
            public readonly PropertyInfo PropertyTypeCollectionSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, PropertyTypeCollection>(x => x.PropertyTypes);
        }

        void PropertyTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyTypeCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Name of the Group, which corresponds to the Tab-name in the UI
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets or sets the Sort Order of the Group
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set { SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector); }
        }

        /// <summary>
        /// Gets or sets a collection of PropertyTypes for this PropertyGroup
        /// </summary>
        [DataMember]
        public PropertyTypeCollection PropertyTypes
        {
            get { return _propertyTypes; }
            set
            {
                _propertyTypes = value;

                //since we're adding this collection to this group, we need to ensure that all the lazy values are set.
                foreach (var propertyType in _propertyTypes)
                {
                    propertyType.PropertyGroupId = new Lazy<int>(() => this.Id);
                }
                
                _propertyTypes.CollectionChanged += PropertyTypesChanged;
            }
        }

        public bool Equals(PropertyGroup other)
        {
            if (base.Equals(other)) return true;

            //Check whether the PropertyGroup's properties are equal. 
            return Name.InvariantEquals(other.Name);
        }

        public override int GetHashCode()
        {
            //Get hash code for the Name field if it is not null. 
            int baseHash = base.GetHashCode();

            //Get hash code for the Alias field. 
            int nameHash = Name.ToLowerInvariant().GetHashCode();

            //Calculate the hash code for the product. 
            return baseHash ^ nameHash;
        }

    }
}