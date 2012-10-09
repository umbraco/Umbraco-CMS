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
    /// Represents the contnet type that a <see cref="Content"/> object is based on
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class ContentType : Entity, IContentType
    {
        private int _parentId;
        private string _name;
        private int _level;
        private string _path;
        private string _alias;
        private string _description;
        private int _sortOrder;
        private string _icon;
        private string _thumbnail;
        private string _defaultTemplate;
        private int _userId;
        private bool _trashed;
        private PropertyGroupCollection _propertyGroups;
        private List<IContentTypeComposition> _contentTypeComposition;
        private IEnumerable<string> _allowedTemplates;
        private IEnumerable<int> _allowedContentTypes;

        public ContentType()
        {
            _allowedTemplates = new List<string>();
            _allowedContentTypes = new List<int>();
            _propertyGroups = new PropertyGroupCollection();
            _contentTypeComposition = new List<IContentTypeComposition>();
        }

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<ContentType, int>(x => x.ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<ContentType, int>(x => x.SortOrder);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<ContentType, int>(x => x.Level);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.Path);
        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.Alias);
        private static readonly PropertyInfo DescriptionSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.Description);
        private static readonly PropertyInfo IconSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.Icon);
        private static readonly PropertyInfo ThumbnailSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.Thumbnail);
        private static readonly PropertyInfo DefaultTemplateSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.DefaultTemplate);
        private static readonly PropertyInfo UserIdSelector = ExpressionHelper.GetPropertyInfo<ContentType, int>(x => x.UserId);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<ContentType, bool>(x => x.Trashed);
        private static readonly PropertyInfo AllowedTemplatesSelector = ExpressionHelper.GetPropertyInfo<ContentType, IEnumerable<string>>(x => x.AllowedTemplates);
        private static readonly PropertyInfo AllowedContentTypesSelector = ExpressionHelper.GetPropertyInfo<ContentType, IEnumerable<int>>(x => x.AllowedContentTypes);
        private static readonly PropertyInfo ContentTypeCompositionSelector = ExpressionHelper.GetPropertyInfo<ContentType, List<IContentTypeComposition>>(x => x.ContentTypeComposition);
        private readonly static PropertyInfo PropertyGroupCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentType, PropertyGroupCollection>(x => x.PropertyGroups);
        void PropertyGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyGroupCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        /// <remarks>Might not be necessary if handled as a relation?</remarks>
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the current entity
        /// </summary>
        [DataMember]
        public string Name
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
        public int Level //NOTE Is this relevant for a ContentType?
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
        public string Path //NOTE Is this relevant for a ContentType?
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
        public string Alias
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
        public string Description
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
        public int SortOrder
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
        public string Icon
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
        public string Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                OnPropertyChanged(ThumbnailSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Path to default Template
        /// </summary>
        [DataMember]
        public string DefaultTemplate
        {
            get { return _defaultTemplate; }
            set
            {
                _defaultTemplate = value;
                OnPropertyChanged(DefaultTemplateSelector);
            }
        }

        /// <summary>
        /// Id of the user who created this Content
        /// </summary>
        [DataMember]
        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged(UserIdSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether this ContentType is Trashed or not.
        /// If ContentType is Trashed it will be located in the Recyclebin.
        /// </summary>
        [DataMember]
        public bool Trashed //NOTE Is this relevant for a ContentType?
        {
            get { return _trashed; }
            set
            {
                _trashed = value;
                OnPropertyChanged(TrashedSelector);
            }
        }

        /// <summary>
        /// Gets or sets a list of aliases for allowed Templates
        /// </summary>
        public IEnumerable<string> AllowedTemplates
        {
            get { return _allowedTemplates; }
            set
            {
                _allowedTemplates = value;
                OnPropertyChanged(AllowedTemplatesSelector);
            }
        }

        /// <summary>
        /// Gets or sets a list of integer Ids for allowed ContentTypes
        /// </summary>
        [DataMember]
        public IEnumerable<int> AllowedContentTypes
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
        public PropertyGroupCollection PropertyGroups
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
        public IEnumerable<PropertyType> PropertyTypes
        {
            get { return PropertyGroups.SelectMany(x => x.PropertyTypes); }
        }

        /// <summary>
        /// List of ContentTypes that make up a composition of PropertyGroups and PropertyTypes for the current ContentType
        /// </summary>
        [DataMember]
        public List<IContentTypeComposition> ContentTypeComposition
        {
            get { return _contentTypeComposition; }
            set
            {
                _contentTypeComposition = value;
                OnPropertyChanged(ContentTypeCompositionSelector);
            }
        }

        /// <summary>
        /// Returns a list of <see cref="PropertyGroup"/> objects from the composition
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> CompositionPropertyGroups
        {
            get
            {
                var groups = PropertyGroups.Union(ContentTypeComposition.SelectMany(x => x.CompositionPropertyGroups));
                return groups;
            }
        }

        /// <summary>
        /// Returns a list of <see cref="PropertyType"/> objects from the composition
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> CompositionPropertyTypes
        {
            get
            {
                var propertyTypes = PropertyTypes.Union(ContentTypeComposition.SelectMany(x => x.CompositionPropertyTypes));
                return propertyTypes;
            }
        }

        /// <summary>
        /// Adds a new ContentType to the list of composite ContentTypes
        /// </summary>
        /// <param name="contentType"><see cref="ContentType"/> to add</param>
        /// <returns>True if ContentType was added, otherwise returns False</returns>
        public bool AddContentType(IContentTypeComposition contentType)
        {
            if(contentType.ContentTypeComposition.Any(x => x.CompositionAliases().Any(ContentTypeCompositionExists)))
                return false;

            if (!ContentTypeCompositionExists(contentType.Alias))
            {
                ContentTypeComposition.Add(contentType);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a ContentType with the supplied alias from the the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="ContentType"/></param>
        /// <returns>True if ContentType was removed, otherwise returns False</returns>
        public bool RemoveContentType(string alias)
        {
            if (!ContentTypeCompositionExists(alias))
            {
                var contentTypeComposition = ContentTypeComposition.First(x => x.Alias == alias);
                return ContentTypeComposition.Remove(contentTypeComposition);
            }
            return false;
        }

        /// <summary>
        /// Checks if a ContentType with the supplied alias exists in the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="ContentType"/></param>
        /// <returns>True if ContentType with alias exists, otherwise returns False</returns>
        public bool ContentTypeCompositionExists(string alias)
        {
            if (ContentTypeComposition.Any(x => x.Alias.Equals(alias)))
                return true;

            if (ContentTypeComposition.Any(x => x.ContentTypeCompositionExists(alias)))
                return true;

            return false;
        }

        /// <summary>
        /// Gets a list of ContentType aliases from the current composition 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Does not contain the alias of the Current ContentType</remarks>
        public IEnumerable<string> CompositionAliases()
        {
            return ContentTypeComposition.Select(x => x.Alias).Union(ContentTypeComposition.SelectMany(x => x.CompositionAliases()));
        }

        //TODO Implement moving PropertyType between groups.
        /*public bool MovePropertyTypeToGroup(string propertyTypeAlias, string groupName)
        {}*/

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();
            Key = Guid.NewGuid();

            if (ParentId == 0)
                _parentId = -1;
        }

        /// <summary>
        /// Method to call when Entity is being updated
        /// </summary>
        /// <remarks>Modified Date is set and a new Version guid is set</remarks>
        internal override void UpdatingEntity()
        {
            base.UpdatingEntity();
        }
    }
}