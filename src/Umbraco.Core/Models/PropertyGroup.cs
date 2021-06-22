using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
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
        private Guid? _parentKey;
        private PropertyGroupType _type;
        private string _name;
        private string _icon;
        private int _sortOrder;
        private PropertyTypeCollection _propertyTypes;

        public PropertyGroup(bool isPublishing)
            : this(new PropertyTypeCollection(isPublishing))
        { }

        public PropertyGroup(PropertyTypeCollection propertyTypeCollection)
        {
            PropertyTypes = propertyTypeCollection;
        }

        private void PropertyTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(PropertyTypes));
        }

        /// <summary>
        /// Gets or sets the parent key of the group.
        /// </summary>
        [DataMember]
        public Guid? ParentKey
        {
            get => _parentKey;
            set => SetPropertyValueAndDetectChanges(value, ref _parentKey, nameof(ParentKey));
        }

        /// <summary>
        /// Gets or sets the Sort Order of the Group
        /// </summary>
        [DataMember]
        public PropertyGroupType Type
        {
            get => _type;
            set => SetPropertyValueAndDetectChanges(value, ref _type, nameof(Type));
        }

        /// <summary>
        /// Gets or sets the Name of the Group, which corresponds to the Tab-name in the UI
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <summary>
        /// Gets or sets the icon of the group.
        /// </summary>
        [DataMember]
        public string Icon
        {
            get => _icon;
            set => SetPropertyValueAndDetectChanges(value, ref _icon, nameof(Icon));
        }

        /// <summary>
        /// Gets or sets the Sort Order of the Group
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
        }

        /// <summary>
        /// Gets or sets a collection of PropertyTypes for this PropertyGroup
        /// </summary>
        /// <remarks>
        /// Marked DoNotClone because we will manually deal with cloning and the event handlers
        /// </remarks>
        [DataMember]
        [DoNotClone]
        public PropertyTypeCollection PropertyTypes
        {
            get => _propertyTypes;
            set
            {
                if (_propertyTypes != null)
                {
                    _propertyTypes.ClearCollectionChangedEvents();
                }
                    
                _propertyTypes = value;

                // since we're adding this collection to this group,
                // we need to ensure that all the lazy values are set.
                foreach (var propertyType in _propertyTypes)
                    propertyType.PropertyGroupId = new Lazy<int>(() => Id);

                OnPropertyChanged(nameof(PropertyTypes));
                _propertyTypes.CollectionChanged += PropertyTypesChanged;
            }
        }

        public bool Equals(PropertyGroup other) => other != null && base.Equals(other) && ParentKey == other.ParentKey && Type == other.Type && Icon == other.Icon && Name.InvariantEquals(other.Name);

        public override int GetHashCode() => (base.GetHashCode(), ParentKey, Type, Icon, Name?.ToLowerInvariant()).GetHashCode();

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (PropertyGroup)clone;

            if (clonedEntity._propertyTypes != null)
            {
                clonedEntity._propertyTypes.ClearCollectionChangedEvents();             //clear this event handler if any
                clonedEntity._propertyTypes = (PropertyTypeCollection) _propertyTypes.DeepClone(); //manually deep clone
                clonedEntity._propertyTypes.CollectionChanged += clonedEntity.PropertyTypesChanged;       //re-assign correct event handler
            }
        }
    }

    internal static class PropertyGroupExtensions
    {
        /// <summary>
        /// Orders the property groups by hierarchy (so child groups are after their parent group) and removes circular references.
        /// </summary>
        /// <param name="propertyGroups">The property groups.</param>
        /// <returns>
        /// The ordered property groups.
        /// </returns>
        public static IEnumerable<PropertyGroup> OrderByHierarchy(this IEnumerable<PropertyGroup> propertyGroups)
        {
            var groups = propertyGroups.ToList();
            var visitedParentKeys = new HashSet<Guid>(groups.Count);

            IEnumerable<PropertyGroup> OrderByHierarchy(Guid? parentKey)
            {
                if (parentKey.HasValue && visitedParentKeys.Add(parentKey.Value) == false)
                {
                    // We already visited this parent key, stop to prevent a circular reference
                    yield break;
                }

                foreach (var group in groups.Where(x => x.ParentKey == parentKey).OrderBy(x => x.Type).ThenBy(x => x.SortOrder))
                {
                    yield return group;

                    foreach (var childGroup in OrderByHierarchy(group.Key))
                    {
                        yield return childGroup;
                    }
                }
            }

            return OrderByHierarchy(null);
        }
    }
}
