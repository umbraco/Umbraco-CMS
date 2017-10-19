﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for base ContentType properties and methods
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
    public abstract class ContentTypeBase : Entity, IContentTypeBase
    {
        private Lazy<int> _parentId;
        private string _name;
        private int _level;
        private string _path;
        private string _alias;
        private string _description;
        private int _sortOrder;
        private string _icon = "icon-folder";
        private string _thumbnail = "folder.png";
        private int _creatorId;
        private bool _allowedAsRoot;
        private bool _isContainer;
        private bool _trashed;
        private PropertyGroupCollection _propertyGroups;
        private PropertyTypeCollection _propertyTypes;
        private IEnumerable<ContentTypeSort> _allowedContentTypes;
        private bool _hasPropertyTypeBeenRemoved;


        protected ContentTypeBase(int parentId)
        {
			Mandate.ParameterCondition(parentId != 0, "parentId");

            _parentId = new Lazy<int>(() => parentId);
            _allowedContentTypes = new List<ContentTypeSort>();
            _propertyGroups = new PropertyGroupCollection();
            _propertyTypes = new PropertyTypeCollection();
            _propertyTypes.CollectionChanged += PropertyTypesChanged;
            _additionalData = new Dictionary<string, object>();
        }

		protected ContentTypeBase(IContentTypeBase parent) : this(parent, null)
		{
		}

        protected ContentTypeBase(IContentTypeBase parent, string alias)
        {
            Mandate.ParameterNotNull(parent, "parent");

            _alias = alias;
            _parentId = new Lazy<int>(() => parent.Id);
            _allowedContentTypes = new List<ContentTypeSort>();
            _propertyGroups = new PropertyGroupCollection();
            _propertyTypes = new PropertyTypeCollection();
            _propertyTypes.CollectionChanged += PropertyTypesChanged;
            _additionalData = new Dictionary<string, object>();
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Name);
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.ParentId);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.SortOrder);
            public readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.Level);
            public readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Path);
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Alias);
            public readonly PropertyInfo DescriptionSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Description);
            public readonly PropertyInfo IconSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Icon);
            public readonly PropertyInfo ThumbnailSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Thumbnail);
            public readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.CreatorId);
            public readonly PropertyInfo AllowedAsRootSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.AllowedAsRoot);
            public readonly PropertyInfo IsContainerSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.IsContainer);
            public readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.Trashed);
            public readonly PropertyInfo AllowedContentTypesSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, IEnumerable<ContentTypeSort>>(x => x.AllowedContentTypes);
            public readonly PropertyInfo PropertyGroupCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, PropertyGroupCollection>(x => x.PropertyGroups);
            public readonly PropertyInfo PropertyTypeCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, IEnumerable<PropertyType>>(x => x.PropertyTypes);
            public readonly PropertyInfo HasPropertyTypeBeenRemovedSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.HasPropertyTypeBeenRemoved);

            //Custom comparer for enumerable
            public readonly DelegateEqualityComparer<IEnumerable<ContentTypeSort>> ContentTypeSortComparer =
                new DelegateEqualityComparer<IEnumerable<ContentTypeSort>>(
                    (sorts, enumerable) => sorts.UnsortedSequenceEqual(enumerable),
                    sorts => sorts.GetHashCode());
        }


        protected void PropertyGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyGroupCollectionSelector);
        }

        protected void PropertyTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyTypeCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        /// <remarks>Might not be necessary if handled as a relation?</remarks>
        [DataMember]
        public virtual int ParentId
        {
            get
            {
				var val = _parentId.Value;
				if (val == 0)
				{
					throw new InvalidOperationException("The ParentId cannot be a value of 0. Perhaps the parent object used to instantiate this object has not been persisted to the data store.");
				}
				return val;				
            }
            set
            {
                _parentId = new Lazy<int>(() => value);
                OnPropertyChanged(Ps.Value.ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the current entity
        /// </summary>
        [DataMember]
        public virtual string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        public virtual int Level //NOTE Is this relevant for a ContentType?
        {
            get { return _level; }
            set { SetPropertyValueAndDetectChanges(value, ref _level, Ps.Value.LevelSelector); }
        }

        /// <summary>
        /// Gets of sets the path
        /// </summary>
        [DataMember]
        public virtual string Path //NOTE Is this relevant for a ContentType?
        {
            get { return _path; }
            set { SetPropertyValueAndDetectChanges(value, ref _path, Ps.Value.PathSelector); }
        }

        /// <summary>
        /// The Alias of the ContentType
        /// </summary>
        [DataMember]
        public virtual string Alias
        {
            get { return _alias; }
            set
            {
                SetPropertyValueAndDetectChanges(
                    value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase), 
                    ref _alias, 
                    Ps.Value.AliasSelector);
            }
        }

        /// <summary>
        /// Description for the ContentType
        /// </summary>
        [DataMember]
        public virtual string Description
        {
            get { return _description; }
            set { SetPropertyValueAndDetectChanges(value, ref _description, Ps.Value.DescriptionSelector); }
        }

        /// <summary>
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        public virtual int SortOrder
        {
            get { return _sortOrder; }
            set { SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector); }
        }

        /// <summary>
        /// Name of the icon (sprite class) used to identify the ContentType
        /// </summary>
        [DataMember]
        public virtual string Icon
        {
            get { return _icon; }
            set { SetPropertyValueAndDetectChanges(value, ref _icon, Ps.Value.IconSelector); }
        }

        /// <summary>
        /// Name of the thumbnail used to identify the ContentType
        /// </summary>
        [DataMember]
        public virtual string Thumbnail
        {
            get { return _thumbnail; }
            set { SetPropertyValueAndDetectChanges(value, ref _thumbnail, Ps.Value.ThumbnailSelector); }
        }

        /// <summary>
        /// Gets or sets the Id of the user who created this ContentType
        /// </summary>
        [DataMember]
        public virtual int CreatorId
        {
            get { return _creatorId; }
            set { SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.CreatorIdSelector); }
        }

        /// <summary>
        /// Gets or Sets a boolean indicating whether this ContentType is allowed at the root
        /// </summary>
        [DataMember]
        public virtual bool AllowedAsRoot
        {
            get { return _allowedAsRoot; }
            set { SetPropertyValueAndDetectChanges(value, ref _allowedAsRoot, Ps.Value.AllowedAsRootSelector); }
        }

        /// <summary>
        /// Gets or Sets a boolean indicating whether this ContentType is a Container
        /// </summary>
        /// <remarks>
        /// ContentType Containers doesn't show children in the tree, but rather in grid-type view.
        /// </remarks>
        [DataMember]
        public virtual bool IsContainer
        {
            get { return _isContainer; }
            set { SetPropertyValueAndDetectChanges(value, ref _isContainer, Ps.Value.IsContainerSelector); }
        }

        /// <summary>
        /// Boolean indicating whether this ContentType is Trashed or not.
        /// If ContentType is Trashed it will be located in the Recyclebin.
        /// </summary>
        [DataMember]
        public virtual bool Trashed //NOTE Is this relevant for a ContentType?
        {
            get { return _trashed; }
            set { SetPropertyValueAndDetectChanges(value, ref _trashed, Ps.Value.TrashedSelector); }
        }

        private readonly IDictionary<string, object> _additionalData;
        /// <summary>
        /// Some entities may expose additional data that other's might not, this custom data will be available in this collection
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDictionary<string, object> IUmbracoEntity.AdditionalData
        {
            get { return _additionalData; }
        }

        /// <summary>
        /// Gets or sets a list of integer Ids for allowed ContentTypes
        /// </summary>
        [DataMember]
        public virtual IEnumerable<ContentTypeSort> AllowedContentTypes
        {
            get { return _allowedContentTypes; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _allowedContentTypes, Ps.Value.AllowedContentTypesSelector, 
                    Ps.Value.ContentTypeSortComparer);                
            }
        }

        /// <summary>
        /// List of PropertyGroups available on this ContentType
        /// </summary>
        /// <remarks>
        /// A PropertyGroup corresponds to a Tab in the UI
        /// </remarks>
        [DataMember]
        public virtual PropertyGroupCollection PropertyGroups
        {
            get { return _propertyGroups; }
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
        public virtual IEnumerable<PropertyType> PropertyTypes
        {
            get
            {
                return _propertyTypes.Union(PropertyGroups.SelectMany(x => x.PropertyTypes));
            }
        }

        /// <summary>
        /// Gets or sets the property types that are not in a group.
        /// </summary>
        public IEnumerable<PropertyType> NoGroupPropertyTypes
        {
            get { return _propertyTypes; }
            set
            {
                _propertyTypes = new PropertyTypeCollection(value);
                _propertyTypes.CollectionChanged += PropertyTypesChanged;
                PropertyTypesChanged(_propertyTypes, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
            get { return _hasPropertyTypeBeenRemoved; }
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
                _propertyTypes.Add(propertyType);                
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
            if (oldPropertyGroup != null)
                oldPropertyGroup.PropertyTypes.RemoveItem(propertyTypeAlias);
            if (newPropertyGroup != null)
                newPropertyGroup.PropertyTypes.Add(propertyType);

            return true;
        }

        /// <summary>
        /// Removes a PropertyType from the current ContentType
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the <see cref="PropertyType"/> to remove</param>
        public void RemovePropertyType(string propertyTypeAlias)
        {
            //check if the property exist in one of our collections
            if (PropertyGroups.Any(group => group.PropertyTypes.Any(pt => pt.Alias == propertyTypeAlias))
                || _propertyTypes.Any(x => x.Alias == propertyTypeAlias))
            {
                //set the flag that a property has been removed
                HasPropertyTypeBeenRemoved = true;
            }

            foreach (var propertyGroup in PropertyGroups)
            {
                propertyGroup.PropertyTypes.RemoveItem(propertyTypeAlias);
            }

            if (_propertyTypes.Any(x => x.Alias == propertyTypeAlias))
            {
                _propertyTypes.RemoveItem(propertyTypeAlias);               
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
                _propertyTypes.Add(property);
            }

            // actually remove the group
            PropertyGroups.RemoveItem(propertyGroupName);
            OnPropertyChanged(Ps.Value.PropertyGroupCollectionSelector);
        }

        /// <summary>
        /// Sets the ParentId from the lazy integer id
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        public void SetLazyParentId(Lazy<int> id)
        {
            _parentId = id;
        }

        /// <summary>
        /// PropertyTypes that are not part of a PropertyGroup
        /// </summary>
        [IgnoreDataMember]
        internal PropertyTypeCollection PropertyTypeCollection
        {
             get { return _propertyTypes; }
        }

        /// <summary>
        /// Indicates whether a specific property on the current <see cref="IContent"/> entity is dirty.
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>True if Property is dirty, otherwise False</returns>
        public override bool IsPropertyDirty(string propertyName)
        {
            bool existsInEntity = base.IsPropertyDirty(propertyName);

            bool anyDirtyGroups = PropertyGroups.Any(x => x.IsPropertyDirty(propertyName));
            bool anyDirtyTypes = PropertyTypes.Any(x => x.IsPropertyDirty(propertyName));

            return existsInEntity || anyDirtyGroups || anyDirtyTypes;
        }

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

        public override object DeepClone()
        {
            var clone = (ContentTypeBase)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to manually wire up the event handlers for the property type collections - we've ensured
            // its ignored from the auto-clone process because its return values are unions, not raw and 
            // we end up with duplicates, see: http://issues.umbraco.org/issue/U4-4842

            clone._propertyTypes = (PropertyTypeCollection)_propertyTypes.DeepClone();
            clone._propertyTypes.CollectionChanged += clone.PropertyTypesChanged;
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}