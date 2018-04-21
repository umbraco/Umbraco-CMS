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
        private Dictionary<string, string> _publishNames;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parent">Parent <see cref="IContent"/> object</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        public Content(string name, IContent parent, IContentType contentType)
            : this(name, parent, contentType, new PropertyCollection())
        { }

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parent">Parent <see cref="IContent"/> object</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="properties">Collection of properties</param>
        public Content(string name, IContent parent, IContentType contentType, PropertyCollection properties)
            : base(name, parent, contentType, properties)
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
        public Content(string name, int parentId, IContentType contentType)
            : this(name, parentId, contentType, new PropertyCollection())
        { }

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parentId">Id of the Parent content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="properties">Collection of properties</param>
        public Content(string name, int parentId, IContentType contentType, PropertyCollection properties)
            : base(name, parentId, contentType, properties)
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

        [IgnoreDataMember]
        public DateTime? PublishDate { get; internal set; }

        [IgnoreDataMember]
        public int? PublisherId { get; internal set; }

        [IgnoreDataMember]
        public ITemplate PublishTemplate { get; internal set; }

        [IgnoreDataMember]
        public string PublishName { get; internal set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IReadOnlyDictionary<string, string> PublishNames => _publishNames ?? NoNames;

        /// <inheritdoc/>
        public string GetPublishName(string culture)
        {
            if (culture == null) return PublishName;
            if (_publishNames == null) return null;
            return _publishNames.TryGetValue(culture, out var name) ? name : null;
        }

        // sets a publish name
        // internal for repositories
        internal void SetPublishName(string culture, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullOrEmptyException(nameof(name));

            if (culture == null)
            {
                PublishName = name;
                return;
            }

            // private method, assume that culture is valid

            if (_publishNames == null)
                _publishNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            _publishNames[culture] = name;
        }

        // clears a publish name
        private void ClearPublishName(string culture)
        {
            if (culture == null)
            {
                PublishName = null;
                return;
            }

            if (_publishNames == null) return;
            _publishNames.Remove(culture);
            if (_publishNames.Count == 0)
                _publishNames = null;
        }

        // clears all publish names
        private void ClearPublishNames()
        {
            PublishName = null;
            _publishNames = null;
        }

        /// <inheritdoc />
        public bool IsCultureAvailable(string culture)
            => !string.IsNullOrWhiteSpace(GetName(culture));

        /// <inheritdoc />
        public bool IsCulturePublished(string culture)
            => !string.IsNullOrWhiteSpace(GetPublishName(culture));

        [IgnoreDataMember]
        public int PublishedVersionId { get; internal set; }

        [DataMember]
        public bool Blueprint { get; internal set; }

        /// <inheritdoc />
        public virtual bool PublishAllValues()
        {
            // the values we want to publish should be valid
            if (ValidateAll().Any())
                return false;

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException($"Cannot publish invariant culture without a name.");
            PublishName = Name;
            foreach (var (culture, name) in Names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidOperationException($"Cannot publish {culture ?? "invariant"} culture without a name.");
                SetPublishName(culture, name);
            }

            // property.PublishAllValues only deals with supported variations (if any)
            foreach (var property in Properties)
                property.PublishAllValues();

            _publishedState = PublishedState.Publishing;
            return true;
        }

        /// <inheritdoc />
        public virtual bool PublishValues(string culture = null, string segment = null)
        {
            // the variation should be supported by the content type
            ContentType.ValidateVariation(culture, segment, throwIfInvalid: true);

            // the values we want to publish should be valid
            if (Validate(culture, segment).Any())
                return false;

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            if (segment == null)
            {
                var name = GetName(culture);
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidOperationException($"Cannot publish {culture ?? "invariant"} culture without a name.");
                SetPublishName(culture, name);
            }

            // property.PublishValue throws on invalid variation, so filter them out
            foreach (var property in Properties.Where(x => x.PropertyType.ValidateVariation(culture, segment, throwIfInvalid: false)))
                property.PublishValue(culture, segment);

            _publishedState = PublishedState.Publishing;
            return true;
        }

        /// <inheritdoc />
        public virtual bool PublishCultureValues(string culture = null)
        {
            // the values we want to publish should be valid
            if (ValidateCulture(culture).Any())
                return false;

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            var name = GetName(culture);
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"Cannot publish {culture ?? "invariant"} culture without a name.");
            SetPublishName(culture, name);

            // property.PublishCultureValues only deals with supported variations (if any)
            foreach (var property in Properties)
                property.PublishCultureValues(culture);

            _publishedState = PublishedState.Publishing;
            return true;
        }

        /// <inheritdoc />
        public virtual void ClearAllPublishedValues()
        {
            // property.ClearPublishedAllValues only deals with supported variations (if any)
            foreach (var property in Properties)
                property.ClearPublishedAllValues();

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            ClearPublishNames();

            _publishedState = PublishedState.Publishing;
        }

        /// <inheritdoc />
        public virtual void ClearPublishedValues(string culture = null, string segment = null)
        {
            // the variation should be supported by the content type
            ContentType.ValidateVariation(culture, segment, throwIfInvalid: true);

            // property.ClearPublishedValue throws on invalid variation, so filter them out
            foreach (var property in Properties.Where(x => x.PropertyType.ValidateVariation(culture, segment, throwIfInvalid: false)))
                property.ClearPublishedValue(culture, segment);

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            ClearPublishName(culture);

            _publishedState = PublishedState.Publishing;
        }

        /// <inheritdoc />
        public virtual void ClearCulturePublishedValues(string culture = null)
        {
            // property.ClearPublishedCultureValues only deals with supported variations (if any)
            foreach (var property in Properties)
                property.ClearPublishedCultureValues(culture);

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            ClearPublishName(culture);

            _publishedState = PublishedState.Publishing;
        }

        private bool CopyingFromSelf(IContent other)
        {
            // copying from the same Id and VersionPk
            return Id == other.Id && VersionId == other.VersionId;
        }

        /// <inheritdoc />
        public virtual void CopyAllValues(IContent other)
        {
            if (other.ContentTypeId != ContentTypeId)
                throw new InvalidOperationException("Cannot copy values from a different content type.");

            // we could copy from another document entirely,
            // or from another version of the same document,
            // in which case there is a special case.
            var published = CopyingFromSelf(other);

            // note: use property.SetValue(), don't assign pvalue.EditValue, else change tracking fails

            // clear all existing properties
            foreach (var property in Properties)
            foreach (var pvalue in property.Values)
                if (property.PropertyType.ValidateVariation(pvalue.Culture, pvalue.Segment, false))
                    property.SetValue(null, pvalue.Culture, pvalue.Segment);

            // copy other properties
            var otherProperties = other.Properties;
            foreach (var otherProperty in otherProperties)
            {
                var alias = otherProperty.PropertyType.Alias;
                foreach (var pvalue in otherProperty.Values)
                {
                    if (!otherProperty.PropertyType.ValidateVariation(pvalue.Culture, pvalue.Segment, false))
                        continue;
                    var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                    SetValue(alias, value, pvalue.Culture, pvalue.Segment);
                }
            }

            // copy names
            ClearNames();
            foreach (var (languageId, name) in other.Names)
                SetName(languageId, name);
            Name = other.Name;
        }

        /// <inheritdoc />
        public virtual void CopyValues(IContent other, string culture = null, string segment = null)
        {
            if (other.ContentTypeId != ContentTypeId)
                throw new InvalidOperationException("Cannot copy values from a different content type.");

            var published = CopyingFromSelf(other);

            // segment is invariant in comparisons
            segment = segment?.ToLowerInvariant();

            // note: use property.SetValue(), don't assign pvalue.EditValue, else change tracking fails

            // clear all existing properties
            foreach (var property in Properties)
            {
                if (!property.PropertyType.ValidateVariation(culture, segment, false))
                    continue;

                foreach (var pvalue in property.Values)
                    if (pvalue.Culture.InvariantEquals(culture) && pvalue.Segment.InvariantEquals(segment))
                        property.SetValue(null, pvalue.Culture, pvalue.Segment);
            }

            // copy other properties
            var otherProperties = other.Properties;
            foreach (var otherProperty in otherProperties)
            {
                if (!otherProperty.PropertyType.ValidateVariation(culture, segment, false))
                    continue;

                var alias = otherProperty.PropertyType.Alias;
                SetValue(alias, otherProperty.GetValue(culture, segment, published), culture, segment);
            }

            // copy name
            SetName(culture, other.GetName(culture));
        }

        /// <inheritdoc />
        public virtual void CopyCultureValues(IContent other, string culture = null)
        {
            if (other.ContentTypeId != ContentTypeId)
                throw new InvalidOperationException("Cannot copy values from a different content type.");

            var published = CopyingFromSelf(other);

            // note: use property.SetValue(), don't assign pvalue.EditValue, else change tracking fails

            // clear all existing properties
            foreach (var property in Properties)
            foreach (var pvalue in property.Values)
                if (pvalue.Culture.InvariantEquals(culture) && property.PropertyType.ValidateVariation(pvalue.Culture, pvalue.Segment, false))
                    property.SetValue(null, pvalue.Culture, pvalue.Segment);

            // copy other properties
            var otherProperties = other.Properties;
            foreach (var otherProperty in otherProperties)
            {
                var alias = otherProperty.PropertyType.Alias;
                foreach (var pvalue in otherProperty.Values)
                {
                    if (pvalue.Culture != culture || !otherProperty.PropertyType.ValidateVariation(pvalue.Culture, pvalue.Segment, false))
                        continue;
                    var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                    SetValue(alias, value, pvalue.Culture, pvalue.Segment);
                }
            }

            // copy name
            SetName(culture, other.GetName(culture));
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
