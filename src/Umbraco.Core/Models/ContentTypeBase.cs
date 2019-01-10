using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for base ContentType properties and methods
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
    public abstract class ContentTypeBase : TreeEntityBase, IContentTypeBase
    {
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private string _alias;
        private string _description;
        private string _icon = "icon-folder";
        private string _thumbnail = "folder.png";
        private bool _allowedAsRoot; // note: only one that's not 'pure element type'
        private bool _isContainer;
        private PropertyGroupCollection _propertyGroups;
        private PropertyTypeCollection _noGroupPropertyTypes;
        private IEnumerable<ContentTypeSort> _allowedContentTypes;
        private bool _hasPropertyTypeBeenRemoved;
        private ContentVariation _variations;

        protected ContentTypeBase(int parentId)
        {
            if (parentId == 0) throw new ArgumentOutOfRangeException(nameof(parentId));
            ParentId = parentId;

            _allowedContentTypes = new List<ContentTypeSort>();
            _propertyGroups = new PropertyGroupCollection();

            // actually OK as IsPublishing is constant
            // ReSharper disable once VirtualMemberCallInConstructor
            _noGroupPropertyTypes = new PropertyTypeCollection(IsPublishing);
            _noGroupPropertyTypes.CollectionChanged += PropertyTypesChanged;

            _variations = ContentVariation.Nothing;
        }

        protected ContentTypeBase(IContentTypeBase parent)
            : this(parent, null)
        { }

        protected ContentTypeBase(IContentTypeBase parent, string alias)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            SetParent(parent);

            _alias = alias;
            _allowedContentTypes = new List<ContentTypeSort>();
            _propertyGroups = new PropertyGroupCollection();

            // actually OK as IsPublishing is constant
            // ReSharper disable once VirtualMemberCallInConstructor
            _noGroupPropertyTypes = new PropertyTypeCollection(IsPublishing);
            _noGroupPropertyTypes.CollectionChanged += PropertyTypesChanged;

            _variations = ContentVariation.Nothing;
        }

        /// <summary>
        /// Gets a value indicating whether the content type is publishing.
        /// </summary>
        /// <remarks>
        /// <para>A publishing content type supports draft and published values for properties.
        /// It is possible to retrieve either the draft (default) or published value of a property.
        /// Setting the value always sets the draft value, which then needs to be published.</para>
        /// <para>A non-publishing content type only supports one value for properties. Getting
        /// the draft or published value of a property returns the same thing, and publishing
        /// a value property has no effect.</para>
        /// </remarks>
        public abstract bool IsPublishing { get; }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Alias);
            public readonly PropertyInfo DescriptionSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Description);
            public readonly PropertyInfo IconSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Icon);
            public readonly PropertyInfo ThumbnailSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Thumbnail);
            public readonly PropertyInfo AllowedAsRootSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.AllowedAsRoot);
            public readonly PropertyInfo IsContainerSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.IsContainer);
            public readonly PropertyInfo AllowedContentTypesSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, IEnumerable<ContentTypeSort>>(x => x.AllowedContentTypes);
            public readonly PropertyInfo PropertyGroupsSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, PropertyGroupCollection>(x => x.PropertyGroups);
            public readonly PropertyInfo PropertyTypesSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, IEnumerable<PropertyType>>(x => x.PropertyTypes);
            public readonly PropertyInfo HasPropertyTypeBeenRemovedSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.HasPropertyTypeBeenRemoved);
            public readonly PropertyInfo VaryBy = ExpressionHelper.GetPropertyInfo<ContentTypeBase, ContentVariation>(x => x.Variations);

            //Custom comparer for enumerable
            public readonly DelegateEqualityComparer<IEnumerable<ContentTypeSort>> ContentTypeSortComparer =
                new DelegateEqualityComparer<IEnumerable<ContentTypeSort>>(
                    (sorts, enumerable) => sorts.UnsortedSequenceEqual(enumerable),
                    sorts => sorts.GetHashCode());
        }

        protected void PropertyGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyGroupsSelector);
        }

        protected void PropertyTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyTypesSelector);
        }

        /// <summary>
        /// The Alias of the ContentType
        /// </summary>
        [DataMember]
        public virtual string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(
                value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase),
                ref _alias,
                Ps.Value.AliasSelector);
        }

        /// <summary>
        /// Description for the ContentType
        /// </summary>
        [DataMember]
        public string Description
        {
            get => _description;
            set => SetPropertyValueAndDetectChanges(value, ref _description, Ps.Value.DescriptionSelector);
        }

        /// <summary>
        /// Name of the icon (sprite class) used to identify the ContentType
        /// </summary>
        [DataMember]
        public string Icon
        {
            get => _icon;
            set => SetPropertyValueAndDetectChanges(value, ref _icon, Ps.Value.IconSelector);
        }

        /// <summary>
        /// Name of the thumbnail used to identify the ContentType
        /// </summary>
        [DataMember]
        public string Thumbnail
        {
            get => _thumbnail;
            set => SetPropertyValueAndDetectChanges(value, ref _thumbnail, Ps.Value.ThumbnailSelector);
        }

        /// <summary>
        /// Gets or Sets a boolean indicating whether this ContentType is allowed at the root
        /// </summary>
        [DataMember]
        public bool AllowedAsRoot
        {
            get => _allowedAsRoot;
            set => SetPropertyValueAndDetectChanges(value, ref _allowedAsRoot, Ps.Value.AllowedAsRootSelector);
        }

        /// <summary>
        /// Gets or Sets a boolean indicating whether this ContentType is a Container
        /// </summary>
        /// <remarks>
        /// ContentType Containers doesn't show children in the tree, but rather in grid-type view.
        /// </remarks>
        [DataMember]
        public bool IsContainer
        {
            get => _isContainer;
            set => SetPropertyValueAndDetectChanges(value, ref _isContainer, Ps.Value.IsContainerSelector);
        }

        /// <summary>
        /// Gets or sets a list of integer Ids for allowed ContentTypes
        /// </summary>
        [DataMember]
        public IEnumerable<ContentTypeSort> AllowedContentTypes
        {
            get => _allowedContentTypes;
            set => SetPropertyValueAndDetectChanges(value, ref _allowedContentTypes, Ps.Value.AllowedContentTypesSelector,
                Ps.Value.ContentTypeSortComparer);
        }

        /// <summary>
        /// Gets or sets the content variation of the content type.
        /// </summary>
        public virtual ContentVariation Variations
        {
            get => _variations;
            set => SetPropertyValueAndDetectChanges(value, ref _variations, Ps.Value.VaryBy);
        }

        /// <inheritdoc />
        public bool SupportsVariation(string culture, string segment, bool wildcards = false)
        {
            // exact validation: cannot accept a 'null' culture if the property type varies
            //  by culture, and likewise for segment
            // wildcard validation: can accept a '*' culture or segment
            return Variations.ValidateVariation(culture, segment, true, wildcards, false);
        }

        /// <inheritdoc />
        public bool SupportsPropertyVariation(string culture, string segment, bool wildcards = false)
        {
            // non-exact validation: can accept a 'null' culture if the property type varies
            //  by culture, and likewise for segment
            // wildcard validation: can accept a '*' culture or segment
            return Variations.ValidateVariation(culture, segment, false, true, false);
        }

        /// <summary>
        /// List of PropertyGroups available on this ContentType
        /// </summary>
        /// <remarks>
        /// <para>A PropertyGroup corresponds to a Tab in the UI</para>
        /// <para>Marked DoNotClone because we will manually deal with cloning and the event handlers</para>
        /// </remarks>
        [DataMember]
        [DoNotClone]
        public PropertyGroupCollection PropertyGroups
        {
            get => _propertyGroups;
            set
            {
                _propertyGroups = value;
                _propertyGroups.CollectionChanged += PropertyGroupsChanged;
                PropertyGroupsChanged(_propertyGroups, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Gets all property types, across all property groups.
        /// </summary>
        [IgnoreDataMember]
        [DoNotClone]
        public IEnumerable<PropertyType> PropertyTypes
        {
            get
            {
                return _noGroupPropertyTypes.Union(PropertyGroups.SelectMany(x => x.PropertyTypes));
            }
        }

        /// <summary>
        /// Gets or sets the property types that are not in a group.
        /// </summary>
        /// <remarks>
        /// Marked DoNotClone because we will manually deal with cloning and the event handlers
        /// </remarks>
        [DoNotClone]
        public IEnumerable<PropertyType> NoGroupPropertyTypes
        {
            get => _noGroupPropertyTypes;
            set
            {
                if (_noGroupPropertyTypes != null)
                    _noGroupPropertyTypes.CollectionChanged -= PropertyTypesChanged;
                _noGroupPropertyTypes = new PropertyTypeCollection(IsPublishing, value);
                _noGroupPropertyTypes.CollectionChanged += PropertyTypesChanged;
                PropertyTypesChanged(_noGroupPropertyTypes, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// A boolean flag indicating if a property type has been removed from this instance.
        /// </summary>
        /// <remarks>
        /// This is currently (specifically) used in order to know that we need to refresh the content cache which
        /// needs to occur when a property has been removed from a content type
        /// </remarks>
        [IgnoreDataMember]
        internal bool HasPropertyTypeBeenRemoved
        {
            get => _hasPropertyTypeBeenRemoved;
            private set
            {
                _hasPropertyTypeBeenRemoved = value;
                OnPropertyChanged(Ps.Value.HasPropertyTypeBeenRemovedSelector);
            }
        }

        /// <summary>
        /// Checks whether a PropertyType with a given alias already exists
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>Returns <c>True</c> if a PropertyType with the passed in alias exists, otherwise <c>False</c></returns>
        public abstract bool PropertyTypeExists(string propertyTypeAlias);

        /// <summary>
        /// Adds a PropertyGroup.
        /// This method will also check if a group already exists with the same name and link it to the parent.
        /// </summary>
        /// <param name="groupName">Name of the PropertyGroup to add</param>
        /// <returns>Returns <c>True</c> if a PropertyGroup with the passed in name was added, otherwise <c>False</c></returns>
        public abstract bool AddPropertyGroup(string groupName);

        /// <summary>
        /// Adds a PropertyType to a specific PropertyGroup
        /// </summary>
        /// <param name="propertyType"><see cref="PropertyType"/> to add</param>
        /// <param name="propertyGroupName">Name of the PropertyGroup to add the PropertyType to</param>
        /// <returns>Returns <c>True</c> if PropertyType was added, otherwise <c>False</c></returns>
        public abstract bool AddPropertyType(PropertyType propertyType, string propertyGroupName);

        /// <summary>
        /// Adds a PropertyType, which does not belong to a PropertyGroup.
        /// </summary>
        /// <param name="propertyType"><see cref="PropertyType"/> to add</param>
        /// <returns>Returns <c>True</c> if PropertyType was added, otherwise <c>False</c></returns>
        public bool AddPropertyType(PropertyType propertyType)
        {
            if (PropertyTypeExists(propertyType.Alias) == false)
            {
                _noGroupPropertyTypes.Add(propertyType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves a PropertyType to a specified PropertyGroup
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType to move</param>
        /// <param name="propertyGroupName">Name of the PropertyGroup to move the PropertyType to</param>
        /// <returns></returns>
        /// <remarks>If <paramref name="propertyGroupName"/> is null then the property is moved back to
        /// "generic properties" ie does not have a tab anymore.</remarks>
        public bool MovePropertyType(string propertyTypeAlias, string propertyGroupName)
        {
            // note: not dealing with alias casing at all here?

            // get property, ensure it exists
            var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if (propertyType == null) return false;

            // get new group, if required, and ensure it exists
            var newPropertyGroup = propertyGroupName == null
                ? null
                : PropertyGroups.FirstOrDefault(x => x.Name == propertyGroupName);
            if (propertyGroupName != null && newPropertyGroup == null) return false;

            // get old group
            var oldPropertyGroup = PropertyGroups.FirstOrDefault(x =>
                x.PropertyTypes.Any(y => y.Alias == propertyTypeAlias));

            // set new group
            propertyType.PropertyGroupId = newPropertyGroup == null ? null : new Lazy<int>(() => newPropertyGroup.Id, false);

            // remove from old group, if any - add to new group, if any
            oldPropertyGroup?.PropertyTypes.RemoveItem(propertyTypeAlias);
            newPropertyGroup?.PropertyTypes.Add(propertyType);

            return true;
        }

        /// <summary>
        /// Removes a PropertyType from the current ContentType
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the <see cref="PropertyType"/> to remove</param>
        public void RemovePropertyType(string propertyTypeAlias)
        {
            //check through each property group to see if we can remove the property type by alias from it
            foreach (var propertyGroup in PropertyGroups)
            {
                if (propertyGroup.PropertyTypes.RemoveItem(propertyTypeAlias))
                {
                    if (!HasPropertyTypeBeenRemoved)
                    {
                        HasPropertyTypeBeenRemoved = true;
                        OnPropertyChanged(Ps.Value.PropertyTypesSelector);
                    }
                    break;
                }
            }

            //check through each local property type collection (not assigned to a tab)
            if (_noGroupPropertyTypes.RemoveItem(propertyTypeAlias))
            {
                if (!HasPropertyTypeBeenRemoved)
                {
                    HasPropertyTypeBeenRemoved = true;
                    OnPropertyChanged(Ps.Value.PropertyTypesSelector);
                }
            }
        }

        /// <summary>
        /// Removes a PropertyGroup from the current ContentType
        /// </summary>
        /// <param name="propertyGroupName">Name of the <see cref="PropertyGroup"/> to remove</param>
        public void RemovePropertyGroup(string propertyGroupName)
        {
            // if no group exists with that name, do nothing
            var group = PropertyGroups[propertyGroupName];
            if (group == null) return;

            // re-assign the group's properties to no group
            foreach (var property in group.PropertyTypes)
            {
                property.PropertyGroupId = null;
                _noGroupPropertyTypes.Add(property);
            }

            // actually remove the group
            PropertyGroups.RemoveItem(propertyGroupName);
            OnPropertyChanged(Ps.Value.PropertyGroupsSelector);
        }

        /// <summary>
        /// PropertyTypes that are not part of a PropertyGroup
        /// </summary>
        [IgnoreDataMember]
        //fixme should we mark this as EditorBrowsable hidden since it really isn't ever used?
        internal PropertyTypeCollection PropertyTypeCollection => _noGroupPropertyTypes;

        /// <summary>
        /// Indicates whether the current entity is dirty.
        /// </summary>
        /// <returns>True if entity is dirty, otherwise False</returns>
        public override bool IsDirty()
        {
            bool dirtyEntity = base.IsDirty();

            bool dirtyGroups = PropertyGroups.Any(x => x.IsDirty());
            bool dirtyTypes = PropertyTypes.Any(x => x.IsDirty());

            return dirtyEntity || dirtyGroups || dirtyTypes;
        }

        /// <summary>
        /// Resets dirty properties by clearing the dictionary used to track changes.
        /// </summary>
        /// <remarks>
        /// Please note that resetting the dirty properties could potentially
        /// obstruct the saving of a new or updated entity.
        /// </remarks>
        public override void ResetDirtyProperties()
        {
            base.ResetDirtyProperties();

            //loop through each property group to reset the property types
            var propertiesReset = new List<int>();

            foreach (var propertyGroup in PropertyGroups)
            {
                propertyGroup.ResetDirtyProperties();
                foreach (var propertyType in propertyGroup.PropertyTypes)
                {
                    propertyType.ResetDirtyProperties();
                    propertiesReset.Add(propertyType.Id);
                }
            }
            //then loop through our property type collection since some might not exist on a property group
            //but don't re-reset ones we've already done.
            foreach (var propertyType in PropertyTypes.Where(x => propertiesReset.Contains(x.Id) == false))
            {
                propertyType.ResetDirtyProperties();
            }
        }

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (ContentTypeBase) clone;

            if (clonedEntity._noGroupPropertyTypes != null)
            {
                //need to manually wire up the event handlers for the property type collections - we've ensured
                // its ignored from the auto-clone process because its return values are unions, not raw and
                // we end up with duplicates, see: http://issues.umbraco.org/issue/U4-4842

                clonedEntity._noGroupPropertyTypes.CollectionChanged -= PropertyTypesChanged;                    //clear this event handler if any
                clonedEntity._noGroupPropertyTypes = (PropertyTypeCollection) _noGroupPropertyTypes.DeepClone(); //manually deep clone
                clonedEntity._noGroupPropertyTypes.CollectionChanged += clonedEntity.PropertyTypesChanged;              //re-assign correct event handler
            }

            if (clonedEntity._propertyGroups != null)
            {
                clonedEntity._propertyGroups.CollectionChanged -= PropertyGroupsChanged;              //clear this event handler if any
                clonedEntity._propertyGroups = (PropertyGroupCollection) _propertyGroups.DeepClone(); //manually deep clone
                clonedEntity._propertyGroups.CollectionChanged += clonedEntity.PropertyGroupsChanged;        //re-assign correct event handler
            }
        }

        public IContentType DeepCloneWithResetIdentities(string alias)
        {
            var clone = (ContentType)DeepClone();
            clone.Alias = alias;
            clone.Key = Guid.Empty;
            foreach (var propertyGroup in clone.PropertyGroups)
            {
                propertyGroup.ResetIdentity();
                propertyGroup.ResetDirtyProperties(false);
            }
            foreach (var propertyType in clone.PropertyTypes)
            {
                propertyType.ResetIdentity();
                propertyType.ResetDirtyProperties(false);
            }

            clone.ResetIdentity();
            clone.ResetDirtyProperties(false);
            return clone;
        }
    }
}
