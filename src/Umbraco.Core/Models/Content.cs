using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Content object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Content : Entity, IContent
    {
        private IContentType _contentType;
        private int _parentId; 
        private string _name;
        private int _sortOrder;
        private int _level;
        private string _path;
        private string _template;
        private int _userId;
        private bool _trashed;
        private bool _published;
        private string _language;
        private int _contentTypeId;
        private PropertyCollection _properties;
        private DateTime? _releaseDate;
        private DateTime? _expireDate;

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="parentId">Id of the Parent content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        public Content(int parentId, IContentType contentType) : this(parentId, contentType, new PropertyCollection())
        {
        }

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="parentId">Id of the Parent content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="properties">Collection of properties</param>
        public Content(int parentId, IContentType contentType, PropertyCollection properties)
        {
            _parentId = parentId;
            _contentTypeId = int.Parse(contentType.Id.ToString(CultureInfo.InvariantCulture));
            _contentType = contentType;
            _properties = properties;
            _properties.EnsurePropertyTypes(PropertyTypes);
            Version = Guid.NewGuid();
        }

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<Content, int>(x => x.ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<Content, int>(x => x.SortOrder);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<Content, int>(x => x.Level);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Path);
        private static readonly PropertyInfo TemplateSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Template);
        private static readonly PropertyInfo UserIdSelector = ExpressionHelper.GetPropertyInfo<Content, int>(x => x.UserId);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<Content, bool>(x => x.Trashed);
        private static readonly PropertyInfo PublishedSelector = ExpressionHelper.GetPropertyInfo<Content, bool>(x => x.Published);
        private static readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Language);
        private static readonly PropertyInfo DefaultContentTypeIdSelector = ExpressionHelper.GetPropertyInfo<Content, int>(x => x.ContentTypeId);
        private static readonly PropertyInfo ReleaseDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ReleaseDate);
        private static readonly PropertyInfo ExpireDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ExpireDate);
        private readonly static PropertyInfo PropertyCollectionSelector = ExpressionHelper.GetPropertyInfo<Content, PropertyCollection>(x => x.Properties);
        void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        /// <remarks>Might not be necessary if handled as a relation?</remarks>
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            private set
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
            set { 
                _name = value;
                OnPropertyChanged(NameSelector);
            }
        }

        [IgnoreDataMember]
        public string UrlName { get { return Name.ToLower().Replace(" ", "-"); } } //TODO Needs to implement proper url casing/syntax

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
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged(LevelSelector);
            }
        }

        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [DataMember]
        public string Path //Setting this value should be handled by the class not the user
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged(PathSelector);
            }
        }

        /// <summary>
        /// Path to the template used by this Content
        /// This is used to override the default one from the ContentType
        /// </summary>
        /// <remarks>If no template is explicitly set on the Content object, the Default template from the ContentType will be returned</remarks>
        [DataMember]
        public virtual string Template
        {
            get
            {
                if (string.IsNullOrEmpty(_template) || _template == null)
                    return _contentType.DefaultTemplate;

                return _template;
            }
            set
            {
                _template = value;
                OnPropertyChanged(TemplateSelector);
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
        /// Gets the current status of the Content
        /// </summary>
        public ContentStatus Status
        {
            get
            {
                if(Trashed)
                    return ContentStatus.Trashed;

                if(ExpireDate.HasValue && DateTime.UtcNow > ExpireDate.Value)
                    return ContentStatus.Expired;

                if(ReleaseDate.HasValue && ReleaseDate.Value > DateTime.UtcNow)
                    return ContentStatus.AwaitingRelease;

                if(Published)
                    return ContentStatus.Published;

                return ContentStatus.Unpublished;
            }
        }

        /// <summary>
        /// Boolean indicating whether this Content is Trashed or not.
        /// If Content is Trashed it will be located in the Recyclebin.
        /// </summary>
        /// <remarks>When content is trashed it should be unpublished</remarks>
        [DataMember]
        public bool Trashed //Setting this value should be handled by the class not the user
        {
            get { return _trashed; }
            internal set
            {
                _trashed = value;
                OnPropertyChanged(TrashedSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether this Content is Published or not
        /// </summary>
        /// <remarks>Setting Published to true/false should be private or internal</remarks>
        [DataMember]
        public bool Published
        {
            get { return _published; }
            internal set
            {
                _published = value;
                OnPropertyChanged(PublishedSelector);
            }
        }

        /// <summary>
        /// Language of the data contained within this Content object
        /// </summary>
        [DataMember]
        internal string Language
        {
            get { return _language; }
            set
            {
                _language = value;
                OnPropertyChanged(LanguageSelector);
            }
        }

        /// <summary>
        /// Guid Id of the curent Version
        /// </summary>
        [DataMember]
        public Guid Version { get; internal set; }

        /// <summary>
        /// Integer Id of the default ContentType
        /// </summary>
        [DataMember]
        public int ContentTypeId
        {
            get { return _contentTypeId; }
            protected set
            {
                _contentTypeId = value;
                OnPropertyChanged(DefaultContentTypeIdSelector);
            }
        }

        /// <summary>
        /// Collection of properties, which make up all the data available for this Content object
        /// </summary>
        /// <remarks>Properties are loaded as part of the Content object graph</remarks>
        [DataMember]
        public PropertyCollection Properties
        {
            get { return _properties; }
            set
            {
                _properties = value;
                _properties.CollectionChanged += PropertiesChanged;
            }
        }

        /// <summary>
        /// Set property values by alias with an annonymous object
        /// </summary>
        [IgnoreDataMember]
        public object PropertyValues
        {
            set
            {
                if (value == null)
                    throw new Exception("No properties has been passed in");

                var propertyInfos = value.GetType().GetProperties();
                foreach (var propertyInfo in propertyInfos)
                {
                    //Check if a PropertyType with alias exists thus being a valid property
                    var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                    if (propertyType == null)
                        throw new Exception(
                            string.Format(
                                "The property alias {0} is not valid, because no PropertyType with this alias exists",
                                propertyInfo.Name));

                    //Check if a Property with the alias already exists in the collection thus being updated or inserted
                    var item = Properties.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                    if (item != null)
                    {
                        item.Value = propertyInfo.GetValue(value, null);
                        //Update item with newly added value
                        Properties.Add(item);
                    }
                    else
                    {
                        //Create new Property to add to collection
                        var property = propertyType.CreatePropertyFromValue(propertyInfo.GetValue(value, null));
                        Properties.Add(property);
                    }
                }
            }
        }

        /// <summary>
        /// List of PropertyGroups available on this Content object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> PropertyGroups { get { return _contentType.CompositionPropertyGroups; } }

        /// <summary>
        /// List of PropertyTypes available on this Content object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> PropertyTypes { get { return _contentType.CompositionPropertyTypes; } }

        /// <summary>
        /// The date this Content should be released and thus be published
        /// </summary>
        [DataMember]
        public DateTime? ReleaseDate
        {
            get { return _releaseDate; }
            set
            {
                if(value.HasValue && value.Value > DateTime.UtcNow && Published)
                    ChangePublishedState(false);

                if (value.HasValue && value.Value < DateTime.UtcNow && !Published)
                    ChangePublishedState(true);

                _releaseDate = value;
                OnPropertyChanged(ReleaseDateSelector);
            }
        }

        /// <summary>
        /// The date this Content should expire and thus be unpublished
        /// </summary>
        [DataMember]
        public DateTime? ExpireDate
        {
            get { return _expireDate; }
            set
            {
                if(value.HasValue && DateTime.UtcNow > value.Value && Published)
                    ChangePublishedState(false);

                _expireDate = value;
                OnPropertyChanged(ExpireDateSelector);
            }
        }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        [IgnoreDataMember]
        public IContentType ContentType
        {
            get { return _contentType; }
        }

        /// <summary>
        /// Changes the <see cref="ContentType"/> for the current content object
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <remarks>Leaves PropertyTypes intact after change</remarks>
        public void ChangeContentType(IContentType contentType)
        {
            ContentTypeId = contentType.Id;
            _contentType = contentType;
            _properties.EnsurePropertyTypes(PropertyTypes);
            _properties.CollectionChanged += PropertiesChanged;
        }

        /// <summary>
        /// Changes the <see cref="ContentType"/> for the current content object and removes PropertyTypes,
        /// which are not part of the new ContentType.
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
        public void ChangeContentType(IContentType contentType, bool clearProperties)
        {
            if(clearProperties)
            {
                ContentTypeId = contentType.Id;
                _contentType = contentType;
                _properties.EnsureCleanPropertyTypes(PropertyTypes);
                _properties.CollectionChanged += PropertiesChanged;
                return;
            }

            ChangeContentType(contentType);
        }

        /// <summary>
        /// Indicates whether the content object has a property with the supplied alias
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>True if Property with given alias exists, otherwise False</returns>
        public bool HasProperty(string propertyTypeAlias)
        {
            return Properties.Contains(propertyTypeAlias);
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as an <see cref="object"/></returns>
        public object GetValue(string propertyTypeAlias)
        {
            return Properties[propertyTypeAlias].Value;
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <typeparam name="TPassType">Type of the value to return</typeparam>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as a <see cref="TPassType"/></returns>
        public TPassType GetValue<TPassType>(string propertyTypeAlias)
        {
            return (TPassType)Properties[propertyTypeAlias].Value;
        }

        /// <summary>
        /// Sets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public void SetValue(string propertyTypeAlias, object value)
        {
            if (Properties.Contains(propertyTypeAlias))
            {
                Properties[propertyTypeAlias].Value = value;
                return;
            }

            var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if(propertyType == null)
            {
                throw new Exception(string.Format("No PropertyType exists with the supplied alias: {0}", propertyTypeAlias));
            }
            Properties.Add(propertyType.CreatePropertyFromValue(value));
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();
            Key = Guid.NewGuid();
        }

        /// <summary>
        /// Method to call when Entity is being updated
        /// </summary>
        /// <remarks>Modified Date is set and a new Version guid is set</remarks>
        internal override void UpdatingEntity()
        {
            base.UpdatingEntity();
            Version = Guid.NewGuid();
        }

        /// <summary>
        /// Changes the Published state of the content object
        /// </summary>
        /// <param name="isPublished">Boolean indicating whether content is published (true) or unpublished (false)</param>
        internal void ChangePublishedState(bool isPublished)
        {
            Published = isPublished;
            //NOTE Should this be checked against the Expire/Release dates?
            //TODO possibly create new (unpublished version)?
        }

        /// <summary>
        /// Changes the Trashed state of the content object
        /// </summary>
        /// <param name="isTrashed">Boolean indicating whether content is trashed (true) or not trashed (false)</param>
        /// <param name="parentId"> </param>
        internal void ChangeTrashedState(bool isTrashed, int parentId = -1)
        {
            Trashed = isTrashed;

            //If Content is trashed the parent id should be set to that of the RecycleBin
            if(isTrashed)
            {
                ParentId = -20;
            }
            else//otherwise set the parent id to the optional parameter, -1 being the fallback
            {
                ParentId = parentId;
            }

            //If the content is trashed and is published it should be marked as unpublished
            if (isTrashed && Published)
            {
                ChangePublishedState(false);
            }
        }
    }
}