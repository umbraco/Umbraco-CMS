using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Content object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Content : ContentBase, IContent
    {
        private ContentScheduleCollection _schedule;
        private int? _templateId;
        private bool _published;
        private PublishedState _publishedState;
        private ContentCultureInfosCollection _publishInfos;
        private ContentCultureInfosCollection _publishInfosOrig;
        private HashSet<string> _editedCultures;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

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

            ContentTypeId = contentType.Id;
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

            ContentTypeId = contentType.Id;
            _publishedState = PublishedState.Unpublished;
            PublishedVersionId = 0;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo TemplateSelector = ExpressionHelper.GetPropertyInfo<Content, int?>(x => x.TemplateId);
            public readonly PropertyInfo PublishedSelector = ExpressionHelper.GetPropertyInfo<Content, bool>(x => x.Published);
            public readonly PropertyInfo ContentScheduleSelector = ExpressionHelper.GetPropertyInfo<Content, ContentScheduleCollection>(x => x.ContentSchedule);
            public readonly PropertyInfo PublishCultureInfosSelector = ExpressionHelper.GetPropertyInfo<Content, IReadOnlyDictionary<string, ContentCultureInfos>>(x => x.PublishCultureInfos);
        }

        public ContentCultureInfosCollection PublishInfos
        {
            get => _publishInfos;
            set => _publishInfos = value;
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
                SetPropertyValueAndDetectChanges(value, ref _schedule, Ps.Value.ContentScheduleSelector);
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
            OnPropertyChanged(Ps.Value.ContentScheduleSelector);
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
        public virtual int? TemplateId
        {
            get => _templateId;
            set => SetPropertyValueAndDetectChanges(value, ref _templateId, Ps.Value.TemplateSelector);
        }


        /// <summary>
        /// Gets or sets a value indicating whether this content item is published or not.
        /// </summary>
        [DataMember]
        public bool Published
        {
            get => _published;

            // the setter is internal and should only be invoked from
            // - the ContentFactory when creating a content entity from a dto
            // - the ContentRepository when updating a content entity
            internal set
            {
                SetPropertyValueAndDetectChanges(value, ref _published, Ps.Value.PublishedSelector);
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
        public bool Edited { get; internal set; }

        /// <inheritdoc />
        [IgnoreDataMember]
        public DateTime? PublishDate { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public int? PublisherId { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public int? PublishTemplateId { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public string PublishName { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public IEnumerable<string> EditedCultures => CultureInfos.Keys.Where(IsCultureEdited);

        /// <inheritdoc />
        [IgnoreDataMember]
        public IEnumerable<string> PublishedCultures => _publishInfos?.Keys ?? Enumerable.Empty<string>();

        /// <inheritdoc />
        public bool IsCulturePublished(string culture)
            // just check _publishInfos
            // a non-available culture could not become published anyways
            => _publishInfos != null && _publishInfos.ContainsKey(culture);

        /// <inheritdoc />
        public bool WasCulturePublished(string culture)
            // just check _publishInfosOrig - a copy of _publishInfos
            // a non-available culture could not become published anyways
            => _publishInfosOrig != null && _publishInfosOrig.ContainsKey(culture);

        // adjust dates to sync between version, cultures etc
        // used by the repo when persisting
        internal void AdjustDates(DateTime date)
        {
            foreach (var culture in PublishedCultures.ToList())
            {
                if (_publishInfos == null || !_publishInfos.TryGetValue(culture, out var publishInfos))
                    continue;

                if (_publishInfosOrig != null && _publishInfosOrig.TryGetValue(culture, out var publishInfosOrig)
                    && publishInfosOrig.Date == publishInfos.Date)
                    continue;

                _publishInfos.AddOrUpdate(culture, publishInfos.Name, date);

                if (CultureInfos.TryGetValue(culture, out var infos))
                    SetCultureInfo(culture, infos.Name, date);
            }
        }

        /// <inheritdoc />
        public bool IsCultureEdited(string culture)
            => IsCultureAvailable(culture) && // is available, and
               (!IsCulturePublished(culture) || // is not published, or
                (_editedCultures != null && _editedCultures.Contains(culture))); // is edited

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IReadOnlyDictionary<string, ContentCultureInfos> PublishCultureInfos => _publishInfos ?? NoInfos;

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



        private void ClearPublishInfo(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            if (_publishInfos == null) return;
            _publishInfos.Remove(culture);
            if (_publishInfos.Count == 0) _publishInfos = null;

            // set the culture to be dirty - it's been modified
            TouchCultureInfo(culture);
        }

        // sets a publish edited
        internal void SetCultureEdited(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));
            if (_editedCultures == null)
                _editedCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _editedCultures.Add(culture.ToLowerInvariant());
        }

        // sets all publish edited
        internal void SetCultureEdited(IEnumerable<string> cultures)
        {
            if (cultures == null)
            {
                _editedCultures = null;
            }
            else
            {
                var editedCultures = new HashSet<string>(cultures.Where(x => !x.IsNullOrWhiteSpace()), StringComparer.OrdinalIgnoreCase);
                _editedCultures = editedCultures.Count > 0 ? editedCultures : null;
            }
        }

        /// <summary>
        /// Handles culture infos collection changes.
        /// </summary>
        public void PublishNamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PublishCultureInfosSelector);
        }

        [IgnoreDataMember]
        public int PublishedVersionId { get; internal set; }

        [DataMember]
        public bool Blueprint { get; internal set; }



        /// <summary>
        /// Changes the <see cref="ContentType"/> for the current content object
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <remarks>Leaves PropertyTypes intact after change</remarks>
        public void ChangeContentType(IContentType contentType)
        {
            ContentTypeId = contentType.Id;
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
        public void ChangeContentType(IContentType contentType, bool clearProperties)
        {
            if(clearProperties)
            {
                ContentTypeId = contentType.Id;
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

            // Make a copy of the _publishInfos, this is purely so that we can detect
            // if this entity's previous culture publish state (regardless of the rememberDirty flag)
            _publishInfosOrig = _publishInfos == null
                ? null
                : new ContentCultureInfosCollection(_publishInfos);

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

        public void SetPublishInfo(string culture, string name, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullOrEmptyException(nameof(name));

            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            if (_publishInfos == null)
            {
                _publishInfos = new ContentCultureInfosCollection();
                _publishInfos.CollectionChanged += PublishNamesCollectionChanged;
            }

            _publishInfos.AddOrUpdate(culture, name, date);
        }

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedContent = (Content)clone;

            clonedContent.ContentTypeId = ContentTypeId;

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
