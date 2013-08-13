using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for base ContentType properties and methods
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class ContentTypeBase : Entity, IContentTypeBase
    {
        private Lazy<int> _parentId;
        private string _name;
        private int _level;
        private string _path;
        private string _alias;
        private string _description;
        private int _sortOrder;
        private string _icon = "folder.png";
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
        }

		protected ContentTypeBase(IContentTypeBase parent)
		{
			Mandate.ParameterNotNull(parent, "parent");

			_parentId = new Lazy<int>(() => parent.Id);
			_allowedContentTypes = new List<ContentTypeSort>();
			_propertyGroups = new PropertyGroupCollection();
            _propertyTypes = new PropertyTypeCollection();
		}

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.SortOrder);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.Level);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Path);
        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Alias);
        private static readonly PropertyInfo DescriptionSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Description);
        private static readonly PropertyInfo IconSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Icon);
        private static readonly PropertyInfo ThumbnailSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, string>(x => x.Thumbnail);
        private static readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, int>(x => x.CreatorId);
        private static readonly PropertyInfo AllowedAsRootSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.AllowedAsRoot);
        private static readonly PropertyInfo IsContainerSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.IsContainer);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.Trashed);
        private static readonly PropertyInfo AllowedContentTypesSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, IEnumerable<ContentTypeSort>>(x => x.AllowedContentTypes);
        private static readonly PropertyInfo PropertyGroupCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, PropertyGroupCollection>(x => x.PropertyGroups);
        private static readonly PropertyInfo PropertyTypeCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, IEnumerable<PropertyType>>(x => x.PropertyTypes);
        private static readonly PropertyInfo HasPropertyTypeBeenRemovedSelector = ExpressionHelper.GetPropertyInfo<ContentTypeBase, bool>(x => x.HasPropertyTypeBeenRemoved);


        protected void PropertyGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyGroupCollectionSelector);
        }

        protected void PropertyTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyTypeCollectionSelector);
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
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the current entity
        /// </summary>
        [DataMember]
        public virtual string Name
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
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        public virtual int Level //NOTE Is this relevant for a ContentType?
        {
            get { return _level; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _level = value;
                    return _level;
                }, _level, LevelSelector);
            }
        }

        /// <summary>
        /// Gets of sets the path
        /// </summary>
        [DataMember]
        public virtual string Path //NOTE Is this relevant for a ContentType?
        {
            get { return _path; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _path = value;
                    return _path;
                }, _path, PathSelector);
            }
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
                SetPropertyValueAndDetectChanges(o =>
                    {
                        _alias = value.ToSafeAlias();
                        return _alias;
                    }, _alias, AliasSelector);
            }
        }

        /// <summary>
        /// Description for the ContentType
        /// </summary>
        [DataMember]
        public virtual string Description
        {
            get { return _description; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _description = value;
                    return _description;
                }, _description, DescriptionSelector);
            }
        }

        /// <summary>
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        public virtual int SortOrder
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
        /// Name of the icon (sprite class) used to identify the ContentType
        /// </summary>
        [DataMember]
        public virtual string Icon
        {
            get { return _icon; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _icon = value;
                    return _icon;
                }, _icon, IconSelector);
            }
        }

        /// <summary>
        /// Name of the thumbnail used to identify the ContentType
        /// </summary>
        [DataMember]
        public virtual string Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _thumbnail = value;
                    return _thumbnail;
                }, _thumbnail, ThumbnailSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Id of the user who created this ContentType
        /// </summary>
        [DataMember]
        public virtual int CreatorId
        {
            get { return _creatorId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _creatorId = value;
                    return _creatorId;
                }, _creatorId, CreatorIdSelector);
            }
        }

        /// <summary>
        /// Gets or Sets a boolean indicating whether this ContentType is allowed at the root
        /// </summary>
        [DataMember]
        public virtual bool AllowedAsRoot
        {
            get { return _allowedAsRoot; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _allowedAsRoot = value;
                    return _allowedAsRoot;
                }, _allowedAsRoot, AllowedAsRootSelector);
            }
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _isContainer = value;
                    return _isContainer;
                }, _isContainer, IsContainerSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether this ContentType is Trashed or not.
        /// If ContentType is Trashed it will be located in the Recyclebin.
        /// </summary>
        [DataMember]
        public virtual bool Trashed //NOTE Is this relevant for a ContentType?
        {
            get { return _trashed; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _trashed = value;
                    return _trashed;
                }, _trashed, TrashedSelector);
            }
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
                SetPropertyValueAndDetectChanges(o =>
                {
                    _allowedContentTypes = value;
                    return _allowedContentTypes;
                }, _allowedContentTypes, AllowedContentTypesSelector);
            }
        }

        /// <summary>
        /// List of PropertyGroups available on this ContentType
        /// </summary>
        /// <remarks>A PropertyGroup corresponds to a Tab in the UI</remarks>
        [DataMember]
        public virtual PropertyGroupCollection PropertyGroups
        {
            get { return _propertyGroups; }
            set
            {
                _propertyGroups = value;
                _propertyGroups.CollectionChanged += PropertyGroupsChanged;
            }
        }

        /// <summary>
        /// List of PropertyTypes available on this ContentType.
        /// This list aggregates PropertyTypes across the PropertyGroups.
        /// </summary>
        [IgnoreDataMember]
        public virtual IEnumerable<PropertyType> PropertyTypes
        {
            get
            {
                var types = _propertyTypes.Union(PropertyGroups.SelectMany(x => x.PropertyTypes));
                return types;
            }
            internal set
            {
                _propertyTypes = new PropertyTypeCollection(value);
                _propertyTypes.CollectionChanged += PropertyTypesChanged;
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
                OnPropertyChanged(HasPropertyTypeBeenRemovedSelector);
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
                _propertyTypes.CollectionChanged += PropertyTypesChanged;
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
        public bool MovePropertyType(string propertyTypeAlias, string propertyGroupName)
        {
            if (PropertyTypes.Any(x => x.Alias == propertyTypeAlias) == false || PropertyGroups.Any(x => x.Name == propertyGroupName) == false)
                return false;

            var propertyType = PropertyTypes.First(x => x.Alias == propertyTypeAlias);
            //The PropertyType already belongs to a PropertyGroup, so we have to remove the PropertyType from that group
            if (PropertyGroups.Any(x => x.PropertyTypes.Any(y => y.Alias == propertyTypeAlias)))
            {
                var oldPropertyGroup = PropertyGroups.First(x => x.PropertyTypes.Any(y => y.Alias == propertyTypeAlias));
                oldPropertyGroup.PropertyTypes.RemoveItem(propertyTypeAlias);
            }

            propertyType.PropertyGroupId = new Lazy<int>(() => default(int));
            propertyType.ResetDirtyProperties();

            var propertyGroup = PropertyGroups.First(x => x.Name == propertyGroupName);
            propertyGroup.PropertyTypes.Add(propertyType);

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
            PropertyGroups.RemoveItem(propertyGroupName);
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
    }
}