using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Content object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Content : ContentBase, IContent
    {
        private int? _templateId;
        private ContentScheduleCollection _schedule;
        private bool _published;
        private PublishedState _publishedState;
        private ContentCultureInfosCollection _publishInfos;
        private HashSet<string> _editedCultures;

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parent">Parent <see cref="IContent"/> object</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="culture">An optional culture.</param>
        public Content(string name, IContent parent, IContentType contentType, string culture = null)
            : this(name, parent, contentType, new PropertyCollection(), culture)
        { }

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parent">Parent <see cref="IContent"/> object</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="properties">Collection of properties</param>
        /// <param name="culture">An optional culture.</param>
        public Content(string name, IContent parent, IContentType contentType, PropertyCollection properties, string culture = null)
            : base(name, parent, contentType, properties, culture)
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));
            ContentType = new SimpleContentType(contentType);
            _publishedState = PublishedState.Unpublished;
            PublishedVersionId = 0;
        }

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parentId">Id of the Parent content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="culture">An optional culture.</param>
        public Content(string name, int parentId, IContentType contentType, string culture = null)
            : this(name, parentId, contentType, new PropertyCollection(), culture)
        { }

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parentId">Id of the Parent content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="properties">Collection of properties</param>
        /// <param name="culture">An optional culture.</param>
        public Content(string name, int parentId, IContentType contentType, PropertyCollection properties, string culture = null)
            : base(name, parentId, contentType, properties, culture)
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));
            ContentType = new SimpleContentType(contentType);
            _publishedState = PublishedState.Unpublished;
            PublishedVersionId = 0;
        }

        /// <inheritdoc />
        [DoNotClone]
        public ContentScheduleCollection ContentSchedule
        {
            get
            {
                if (_schedule == null)
                {
                    _schedule = new ContentScheduleCollection();
                    _schedule.CollectionChanged += ScheduleCollectionChanged;
                }
                return _schedule;
            }
            set
            {
                if(_schedule != null)
                    _schedule.CollectionChanged -= ScheduleCollectionChanged;
                SetPropertyValueAndDetectChanges(value, ref _schedule, nameof(ContentSchedule));
                if (_schedule != null)
                    _schedule.CollectionChanged += ScheduleCollectionChanged;
            }
        }

        /// <summary>
        /// Collection changed event handler to ensure the schedule field is set to dirty when the schedule changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ContentSchedule));
        }

        /// <summary>
        /// Gets or sets the template used by the Content.
        /// This is used to override the default one from the ContentType.
        /// </summary>
        /// <remarks>
        /// If no template is explicitly set on the Content object,
        /// the Default template from the ContentType will be returned.
        /// </remarks>
        [DataMember]
        public int? TemplateId
        {
            get => _templateId;
            set => SetPropertyValueAndDetectChanges(value, ref _templateId, nameof(TemplateId));
        }

        /// <summary>
        /// Gets or sets a value indicating whether this content item is published or not.
        /// </summary>
        /// <remarks>
        /// the setter is should only be invoked from
        /// - the ContentFactory when creating a content entity from a dto
        /// - the ContentRepository when updating a content entity
        /// </remarks>
        [DataMember]
        public bool Published
        {
            get => _published;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _published, nameof(Published));
                _publishedState = _published ? PublishedState.Published : PublishedState.Unpublished;
            }
        }

        /// <summary>
        /// Gets the published state of the content item.
        /// </summary>
        /// <remarks>The state should be Published or Unpublished, depending on whether Published
        /// is true or false, but can also temporarily be Publishing or Unpublishing when the
        /// content item is about to be saved.</remarks>
        [DataMember]
        public PublishedState PublishedState
        {
            get => _publishedState;
            set
            {
                if (value != PublishedState.Publishing && value != PublishedState.Unpublishing)
                    throw new ArgumentException("Invalid state, only Publishing and Unpublishing are accepted.");
                _publishedState = value;
            }
        }

        [IgnoreDataMember]
        public bool Edited { get; set; }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        [IgnoreDataMember]
        public ISimpleContentType ContentType { get; private set; }

        /// <inheritdoc />
        [IgnoreDataMember]
        public DateTime? PublishDate { get; set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public int? PublisherId { get; set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public int? PublishTemplateId { get; set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public string PublishName { get; set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public IEnumerable<string> EditedCultures
        {
            get => CultureInfos.Keys.Where(IsCultureEdited);
            set => _editedCultures = value == null ? null : new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        [IgnoreDataMember]
        public IEnumerable<string> PublishedCultures => _publishInfos?.Keys ?? Enumerable.Empty<string>();

        /// <inheritdoc />
        public bool IsCulturePublished(string culture)
            // just check _publishInfos
            // a non-available culture could not become published anyways
            => _publishInfos != null && _publishInfos.ContainsKey(culture);
        
        /// <inheritdoc />
        public bool IsCultureEdited(string culture)
            => IsCultureAvailable(culture) && // is available, and
               (!IsCulturePublished(culture) || // is not published, or
                (_editedCultures != null && _editedCultures.Contains(culture))); // is edited

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ContentCultureInfosCollection PublishCultureInfos
        {
            get
            {
                if (_publishInfos != null) return _publishInfos;
                _publishInfos = new ContentCultureInfosCollection();
                _publishInfos.CollectionChanged += PublishNamesCollectionChanged;
                return _publishInfos;
            }
            set
            {
                if (_publishInfos != null) _publishInfos.CollectionChanged -= PublishNamesCollectionChanged;
                _publishInfos = value;
                if (_publishInfos != null)
                    _publishInfos.CollectionChanged += PublishNamesCollectionChanged;
            }
        }

        /// <inheritdoc/>
        public string GetPublishName(string culture)
        {
            if (culture.IsNullOrWhiteSpace()) return PublishName;
            if (!ContentTypeBase.VariesByCulture()) return null;
            if (_publishInfos == null) return null;
            return _publishInfos.TryGetValue(culture, out var infos) ? infos.Name : null;
        }

        /// <inheritdoc />
        public DateTime? GetPublishDate(string culture)
        {
            if (culture.IsNullOrWhiteSpace()) return PublishDate;
            if (!ContentTypeBase.VariesByCulture()) return null;
            if (_publishInfos == null) return null;
            return _publishInfos.TryGetValue(culture, out var infos) ? infos.Date : (DateTime?) null;
        }
        
        /// <summary>
        /// Handles culture infos collection changes.
        /// </summary>
        private void PublishNamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(PublishCultureInfos));
        }

        [IgnoreDataMember]
        public int PublishedVersionId { get; set; }

        [DataMember]
        public bool Blueprint { get; set; }

        /// <summary>
        /// Changes the <see cref="ContentType"/> for the current content object
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <remarks>Leaves PropertyTypes intact after change</remarks>
        internal void ChangeContentType(IContentType contentType)
        {
            ContentTypeId = contentType.Id;
            ContentType = new SimpleContentType(contentType);
            ContentTypeBase = contentType;
            Properties.EnsurePropertyTypes(PropertyTypes);

            Properties.CollectionChanged -= PropertiesChanged; // be sure not to double add
            Properties.CollectionChanged += PropertiesChanged;
        }

        /// <summary>
        /// Changes the <see cref="ContentType"/> for the current content object and removes PropertyTypes,
        /// which are not part of the new ContentType.
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
        internal void ChangeContentType(IContentType contentType, bool clearProperties)
        {
            if(clearProperties)
            {
                ContentTypeId = contentType.Id;
                ContentType = new SimpleContentType(contentType);
                ContentTypeBase = contentType;
                Properties.EnsureCleanPropertyTypes(PropertyTypes);

                Properties.CollectionChanged -= PropertiesChanged; // be sure not to double add
                Properties.CollectionChanged += PropertiesChanged;
                return;
            }

            ChangeContentType(contentType);
        }

        public override void ResetDirtyProperties(bool rememberDirty)
        {
            base.ResetDirtyProperties(rememberDirty);

            // take care of the published state
            _publishedState = _published ? PublishedState.Published : PublishedState.Unpublished;

            if (_publishInfos == null) return;

            foreach (var infos in _publishInfos)
                infos.ResetDirtyProperties(rememberDirty);
        }

        /// <summary>
        /// Creates a deep clone of the current entity with its identity and it's property identities reset
        /// </summary>
        /// <returns></returns>
        public IContent DeepCloneWithResetIdentities()
        {
            var clone = (Content)DeepClone();
            clone.Key = Guid.Empty;
            clone.VersionId = clone.PublishedVersionId = 0;
            clone.ResetIdentity();

            foreach (var property in clone.Properties)
                property.ResetIdentity();

            return clone;
        }

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedContent = (Content)clone;

            //need to manually clone this since it's not settable
            clonedContent.ContentType = ContentType;

            //if culture infos exist then deal with event bindings
            if (clonedContent._publishInfos != null)
            {
                clonedContent._publishInfos.CollectionChanged -= PublishNamesCollectionChanged;          //clear this event handler if any
                clonedContent._publishInfos = (ContentCultureInfosCollection) _publishInfos.DeepClone(); //manually deep clone
                clonedContent._publishInfos.CollectionChanged += clonedContent.PublishNamesCollectionChanged;    //re-assign correct event handler
            }

            //if properties exist then deal with event bindings
            if (clonedContent._schedule != null)
            {
                clonedContent._schedule.CollectionChanged -= ScheduleCollectionChanged;         //clear this event handler if any
                clonedContent._schedule = (ContentScheduleCollection)_schedule.DeepClone();     //manually deep clone
                clonedContent._schedule.CollectionChanged += clonedContent.ScheduleCollectionChanged;   //re-assign correct event handler
            }
        }
    }
}
