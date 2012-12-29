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
        private string _icon;
        private string _thumbnail;
        private int _creatorId;
        private bool _allowedAsRoot;
        private bool _isContainer;
        private bool _trashed;
        private PropertyGroupCollection _propertyGroups;
        private IEnumerable<ContentTypeSort> _allowedContentTypes;

        protected ContentTypeBase(int parentId)
        {
			Mandate.ParameterCondition(parentId != 0, "parentId");

            _parentId = new Lazy<int>(() => parentId);
            _allowedContentTypes = new List<ContentTypeSort>();
            _propertyGroups = new PropertyGroupCollection();
        }

		protected ContentTypeBase(IContentTypeBase parent)
		{
			Mandate.ParameterNotNull(parent, "parent");

			_parentId = new Lazy<int>(() => parent.Id);
			_allowedContentTypes = new List<ContentTypeSort>();
			_propertyGroups = new PropertyGroupCollection();
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

        protected void PropertyGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyGroupCollectionSelector);
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

        public void SetLazyParentId(Lazy<int> id)
        {
            _parentId = id;
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
                _name = value;
                OnPropertyChanged(NameSelector);
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
                _level = value;
                OnPropertyChanged(LevelSelector);
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
                _path = value;
                OnPropertyChanged(PathSelector);
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
                _alias = value;
                OnPropertyChanged(AliasSelector);
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
                _description = value;
                OnPropertyChanged(DescriptionSelector);
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
                _sortOrder = value;
                OnPropertyChanged(SortOrderSelector);
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
                _icon = value;
                OnPropertyChanged(IconSelector);
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
                _thumbnail = value;
                OnPropertyChanged(ThumbnailSelector);
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
                _creatorId = value;
                OnPropertyChanged(CreatorIdSelector);
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
                _allowedAsRoot = value;
                OnPropertyChanged(AllowedAsRootSelector);
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
                _isContainer = value;
                OnPropertyChanged(IsContainerSelector);
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
                _trashed = value;
                OnPropertyChanged(TrashedSelector);
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
                _allowedContentTypes = value;
                OnPropertyChanged(AllowedContentTypesSelector);
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
            get { return PropertyGroups.SelectMany(x => x.PropertyTypes); }
        }

        //TODO Implement moving PropertyType between groups.
        /*public bool MovePropertyTypeToGroup(string propertyTypeAlias, string groupName)
        {}*/
    }
}