using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private IContentType _contentType;
        private ITemplate _template;
        private bool _published;
        private PublishedState _publishedState;
        private DateTime? _releaseDate;
        private DateTime? _expireDate;
        private Dictionary<string, (string Name, DateTime Date)> _publishInfos;
        private Dictionary<string, (string Name, DateTime Date)> _publishInfosOrig;
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
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
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
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _publishedState = PublishedState.Unpublished;
            PublishedVersionId = 0;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo TemplateSelector = ExpressionHelper.GetPropertyInfo<Content, ITemplate>(x => x.Template);
            public readonly PropertyInfo PublishedSelector = ExpressionHelper.GetPropertyInfo<Content, bool>(x => x.Published);
            public readonly PropertyInfo ReleaseDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ReleaseDate);
            public readonly PropertyInfo ExpireDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ExpireDate);
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
        public virtual ITemplate Template
        {
            get => _template ?? _contentType.DefaultTemplate;
            set => SetPropertyValueAndDetectChanges(value, ref _template, Ps.Value.TemplateSelector);
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

                if(ExpireDate.HasValue && ExpireDate.Value > DateTime.MinValue && DateTime.Now > ExpireDate.Value)
                    return ContentStatus.Expired;

                if(ReleaseDate.HasValue && ReleaseDate.Value > DateTime.MinValue && ReleaseDate.Value > DateTime.Now)
                    return ContentStatus.AwaitingRelease;

                if(Published)
                    return ContentStatus.Published;

                return ContentStatus.Unpublished;
            }
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

        /// <summary>
        /// The date this Content should be released and thus be published
        /// </summary>
        [DataMember]
        public DateTime? ReleaseDate
        {
            get => _releaseDate;
            set => SetPropertyValueAndDetectChanges(value, ref _releaseDate, Ps.Value.ReleaseDateSelector);
        }

        /// <summary>
        /// The date this Content should expire and thus be unpublished
        /// </summary>
        [DataMember]
        public DateTime? ExpireDate
        {
            get => _expireDate;
            set => SetPropertyValueAndDetectChanges(value, ref _expireDate, Ps.Value.ExpireDateSelector);
        }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        [IgnoreDataMember]
        public IContentType ContentType => _contentType;

        /// <inheritdoc />
        [IgnoreDataMember]
        public DateTime? PublishDate { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public int? PublisherId { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public ITemplate PublishTemplate { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public string PublishName { get; internal set; } // set by persistence

        /// <inheritdoc />
        [IgnoreDataMember]
        public IEnumerable<string> EditedCultures => CultureNames.Keys.Where(IsCultureEdited);

        /// <inheritdoc />
        [IgnoreDataMember]
        public IEnumerable<string> PublishedCultures => _publishInfos?.Keys ?? Enumerable.Empty<string>();

        /// <inheritdoc />
        public bool IsCulturePublished(string culture)
            // just check _publishInfos
            // a non-available culture could not become published anyways
            =>  _publishInfos != null && _publishInfos.ContainsKey(culture); 

        /// <inheritdoc />
        public bool WasCulturePublished(string culture)
            // just check _publishInfosOrig - a copy of _publishInfos
            // a non-available culture could not become published anyways
            => _publishInfosOrig != null && _publishInfosOrig.ContainsKey(culture); 

        /// <inheritdoc />
        public bool IsCultureEdited(string culture)
            => IsCultureAvailable(culture) && // is available, and
               (!IsCulturePublished(culture) || // is not published, or
                (_editedCultures != null && _editedCultures.Contains(culture))); // is edited

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IReadOnlyDictionary<string, string> PublishNames => _publishInfos?.ToDictionary(x => x.Key, x => x.Value.Name, StringComparer.OrdinalIgnoreCase) ?? NoNames;

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

        // internal for repository
        internal void SetPublishInfo(string culture, string name, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullOrEmptyException(nameof(name));

            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            if (_publishInfos == null)
                _publishInfos = new Dictionary<string, (string Name, DateTime Date)>(StringComparer.OrdinalIgnoreCase);

            _publishInfos[culture.ToLowerInvariant()] = (name, date);
        }

        private void ClearPublishInfos()
        {
            _publishInfos = null;
        }

        private void ClearPublishInfo(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            if (_publishInfos == null) return;
            _publishInfos.Remove(culture);
            if (_publishInfos.Count == 0) _publishInfos = null;
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

        [IgnoreDataMember]
        public int PublishedVersionId { get; internal set; }

        [DataMember]
        public bool Blueprint { get; internal set; }

        /// <inheritdoc />
        public virtual bool PublishCulture(string culture = "*")
        {
            culture = culture.NullOrWhiteSpaceAsNull();

            // the variation should be supported by the content type properties
            //  if the content type is invariant, only '*' and 'null' is ok
            //  if the content type varies, everything is ok because some properties may be invariant
            if (!ContentType.SupportsPropertyVariation(culture, "*", true))
                throw new NotSupportedException($"Culture \"{culture}\" is not supported by content type \"{ContentType.Alias}\" with variation \"{ContentType.Variations}\".");

            // the values we want to publish should be valid
            if (ValidateProperties(culture).Any())
                return false;

            var alsoInvariant = false;
            if (culture == "*") // all cultures
            {
                foreach (var c in AvailableCultures)
                {
                    var name = GetCultureName(c);
                    if (string.IsNullOrWhiteSpace(name))
                        return false;
                    SetPublishInfo(c, name, DateTime.Now);
                }
            }
            else // one single culture
            {
                var name = GetCultureName(culture);
                if (string.IsNullOrWhiteSpace(name))
                    return false;
                SetPublishInfo(culture, name, DateTime.Now);
                alsoInvariant = true; // we also want to publish invariant values
            }

            // property.PublishValues only publishes what is valid, variation-wise
            foreach (var property in Properties)
            {
                property.PublishValues(culture);
                if (alsoInvariant)
                    property.PublishValues(null);
            }

            _publishedState = PublishedState.Publishing;
            return true;
        }

        /// <inheritdoc />
        public virtual void UnpublishCulture(string culture = "*")
        {
            culture = culture.NullOrWhiteSpaceAsNull();

            // the variation should be supported by the content type properties
            if (!ContentType.SupportsPropertyVariation(culture, "*", true))
                throw new NotSupportedException($"Culture \"{culture}\" is not supported by content type \"{ContentType.Alias}\" with variation \"{ContentType.Variations}\".");

            if (culture == "*") // all cultures
                ClearPublishInfos();
            else // one single culture
                ClearPublishInfo(culture);

            // property.PublishValues only publishes what is valid, variation-wise
            foreach (var property in Properties)
                property.UnpublishValues(culture);

            _publishedState = PublishedState.Publishing;
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

        public override void ResetDirtyProperties(bool rememberDirty)
        {
            base.ResetDirtyProperties(rememberDirty);

            // take care of the published state
            _publishedState = _published ? PublishedState.Published : PublishedState.Unpublished;

            // take care of publish infos
            _publishInfosOrig = _publishInfos == null
                ? null
                : new Dictionary<string, (string Name, DateTime Date)>(_publishInfos, StringComparer.OrdinalIgnoreCase);
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

        public override object DeepClone()
        {
            var clone = (Content) base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to manually clone this since it's not settable
            clone._contentType = (IContentType)ContentType.DeepClone();
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}
