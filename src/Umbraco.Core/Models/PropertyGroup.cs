using System;
using System.Collections.Specialized;
using System.Diagnostics;
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
        private Lazy<int?> _parentId;
        private int _sortOrder;
        private PropertyTypeCollection _propertyTypes;

        public PropertyGroup() : this(new PropertyTypeCollection())
        {
        }

        public PropertyGroup(PropertyTypeCollection propertyTypeCollection)
        {
            PropertyTypes = propertyTypeCollection;
        }

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, int?>(x => x.ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, int>(x => x.SortOrder);
        private readonly static PropertyInfo PropertyTypeCollectionSelector = ExpressionHelper.GetPropertyInfo<PropertyGroup, PropertyTypeCollection>(x => x.PropertyTypes);
        void PropertyTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyTypeCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Name of the Group, which corresponds to the Tab-name in the UI
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Id of the Parent PropertyGroup.
        /// </summary>
        /// <remarks>
        /// A Parent PropertyGroup corresponds to an inherited PropertyGroup from a composition.
        /// If a PropertyType is inserted into an inherited group then a new group will be created with an Id reference to the parent.
        /// </remarks>
        [DataMember]
        public int? ParentId
        {
            get
            {
                if (_parentId == null)
                    return default(int?);
                return _parentId.Value;
            }
            set
            {
                _parentId = new Lazy<int?>(() => value);
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Sort Order of the Group
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _sortOrder = value;
                    return _sortOrder;
                }, _sortOrder, SortOrderSelector);
            }
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
                _propertyTypes.CollectionChanged += PropertyTypesChanged;
            }
        }

        /// <summary>
        /// Sets the ParentId from the lazy integer id
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        internal void SetLazyParentId(Lazy<int?> id)
        {
            _parentId = id;
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