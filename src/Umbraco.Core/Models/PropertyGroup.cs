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
    /// Represents a group of property types.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
    public class PropertyGroup : EntityBase, IEquatable<PropertyGroup>
    {
        private PropertyGroupType _type;
        private string _name;
        private string _alias;
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
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMember]
        public PropertyGroupType Type
        {
            get => _type;
            set => SetPropertyValueAndDetectChanges(value, ref _type, nameof(Type));
        }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <summary>
        /// Gets or sets the alias of the group.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        [DataMember]
        public string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(value, ref _alias, nameof(Alias));
        }

        /// <summary>
        /// Gets or sets the sort order of the group.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
        }

        /// <summary>
        /// Gets or sets a collection of property types for the group.
        /// </summary>
        /// <value>
        /// The property types.
        /// </value>
        /// <remarks>
        /// Marked with DoNotClone, because we will manually deal with cloning and the event handlers.
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

        public bool Equals(PropertyGroup other) => base.Equals(other) || (other != null && Type == other.Type && Alias == other.Alias);

        public override int GetHashCode() => (base.GetHashCode(), Type, Alias).GetHashCode();

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (PropertyGroup)clone;

            if (clonedEntity._propertyTypes != null)
            {
                clonedEntity._propertyTypes.ClearCollectionChangedEvents(); //clear this event handler if any
                clonedEntity._propertyTypes = (PropertyTypeCollection) _propertyTypes.DeepClone(); //manually deep clone
                clonedEntity._propertyTypes.CollectionChanged += clonedEntity.PropertyTypesChanged; //re-assign correct event handler
            }
        }
    }

    internal static class PropertyGroupExtensions
    {
        private const char aliasSeparator = '/';

        internal static string GetParentAlias(string alias)
        {
            var lastIndex = alias?.LastIndexOf(aliasSeparator) ?? -1;
            if (lastIndex == -1)
            {
                return null;
            }

            return alias.Substring(0, lastIndex);
        }

        public static string GetParentAlias(this PropertyGroup propertyGroup) => GetParentAlias(propertyGroup.Alias);

        public static void UpdateParentAlias(this PropertyGroup propertyGroup, string parentAlias)
        {
            // Get current alias without parent
            var alias = propertyGroup.Alias;
            var lastIndex = alias?.LastIndexOf(aliasSeparator) ?? -1;
            if (lastIndex != -1)
            {
                alias = alias.Substring(lastIndex + 1);
            }

            // Update alias
            if (string.IsNullOrEmpty(parentAlias))
            {
                propertyGroup.Alias = alias;
            }
            else
            {
                propertyGroup.Alias = parentAlias + aliasSeparator + alias;
            }
        }

        /// <summary>
        /// Orders the property groups by hierarchy (so child groups are after their parent group).
        /// </summary>
        /// <param name="propertyGroups">The property groups.</param>
        /// <returns>
        /// The ordered property groups.
        /// </returns>
        public static IEnumerable<PropertyGroup> OrderByHierarchy(this IEnumerable<PropertyGroup> propertyGroups)
        {
            var groupsByParentAlias = propertyGroups.ToLookup(x => x.GetParentAlias());

            IEnumerable<PropertyGroup> OrderByHierarchy(string parentAlias)
            {
                foreach (var group in groupsByParentAlias[parentAlias].OrderBy(x => x.SortOrder))
                {
                    yield return group;

                    foreach (var childGroup in OrderByHierarchy(group.Alias))
                    {
                        yield return childGroup;
                    }
                }
            }

            return OrderByHierarchy(null);
        }
    }
}
