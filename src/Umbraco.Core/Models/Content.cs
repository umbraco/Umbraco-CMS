using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Content object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Content : ContentBase, IContent
    {
        private IContentType _contentType;
        private string _template;
        private bool _published;
        private string _language;
        private DateTime? _releaseDate;
        private DateTime? _expireDate;
        private int _writer;

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
        public Content(int parentId, IContentType contentType, PropertyCollection properties) : base(parentId, contentType, properties)
        {
            Mandate.ParameterNotNull(contentType, "contentType");

            _contentType = contentType;
        }
        
        private static readonly PropertyInfo TemplateSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Template);
        private static readonly PropertyInfo PublishedSelector = ExpressionHelper.GetPropertyInfo<Content, bool>(x => x.Published);
        private static readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Language);
        private static readonly PropertyInfo ReleaseDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ReleaseDate);
        private static readonly PropertyInfo ExpireDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ExpireDate);
        private static readonly PropertyInfo WriterSelector = ExpressionHelper.GetPropertyInfo<Content, int>(x => x.WriterId);

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
        /// Gets the current status of the Content
        /// </summary>
        [IgnoreDataMember]
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
        /// The date this Content should be released and thus be published
        /// </summary>
        [DataMember]
        public DateTime? ReleaseDate
        {
            get { return _releaseDate; }
            set
            {
                if(value.HasValue && value.Value > DateTime.UtcNow && Published)
                    _published = false;

                if (value.HasValue && value.Value < DateTime.UtcNow && !Published)
                    _published = true;

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
                    _published = false;

                _expireDate = value;
                OnPropertyChanged(ExpireDateSelector);
            }
        }

        /// <summary>
        /// Id of the user who wrote/updated this Content
        /// </summary>
        [DataMember]
        public virtual int WriterId
        {
            get { return _writer; }
            set
            {
                _writer = value;
                OnPropertyChanged(WriterSelector);
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
            ContentTypeBase = contentType;
            Properties.EnsurePropertyTypes(PropertyTypes);
            Properties.CollectionChanged += PropertiesChanged;
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
                ContentTypeBase = contentType;
                Properties.EnsureCleanPropertyTypes(PropertyTypes);
                Properties.CollectionChanged += PropertiesChanged;
                return;
            }

            ChangeContentType(contentType);
        }

        /// <summary>
        /// Changes the Published state of the content object
        /// </summary>
        /// <param name="isPublished">Boolean indicating whether content is published (true) or unpublished (false)</param>
        public void ChangePublishedState(bool isPublished)
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
        public void ChangeTrashedState(bool isTrashed, int parentId = -1)
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

        /// <summary>
        /// Creates a clone of the current entity
        /// </summary>
        /// <returns></returns>
        public IContent Clone()
        {
            var clone = (Content)this.MemberwiseClone();
            clone.Key = Guid.Empty;
            clone.Version = Guid.NewGuid();
            clone.ResetIdentity();

            return clone;
        }

        /// <summary>
        /// Indicates whether a specific property on the current <see cref="IContent"/> entity is dirty.
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>True if Property is dirty, otherwise False</returns>
        public override bool IsPropertyDirty(string propertyName)
        {
            bool existsInEntity = base.IsPropertyDirty(propertyName);

            bool anyDirtyProperties = Properties.Any(x => x.IsPropertyDirty(propertyName));

            return existsInEntity || anyDirtyProperties;
        }

        /// <summary>
        /// Indicates whether the current entity is dirty.
        /// </summary>
        /// <returns>True if entity is dirty, otherwise False</returns>
        public override bool IsDirty()
        {
            bool dirtyEntity = base.IsDirty();

            bool dirtyProperties = Properties.Any(x => x.IsDirty());

            return dirtyEntity || dirtyProperties;
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

            foreach (var property in Properties)
            {
                property.ResetDirtyProperties();
            }
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
    }
}