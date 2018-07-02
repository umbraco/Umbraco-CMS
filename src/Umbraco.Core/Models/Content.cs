﻿using System;
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
        private HashSet<string> _edited;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parent">Parent <see cref="IContent"/> object</param>
        /// <param name="contentType">ContentType for the current Content object</param>
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

        [IgnoreDataMember]
        public DateTime? PublishDate { get; internal set; }

        [IgnoreDataMember]
        public int? PublisherId { get; internal set; }

        [IgnoreDataMember]
        public ITemplate PublishTemplate { get; internal set; }

        [IgnoreDataMember]
        public string PublishName { get; internal set; }

        // sets publish infos
        // internal for repositories
        // clear by clearing name
        internal void SetPublishInfos(string culture, string name, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullOrEmptyException(nameof(name));

            // this is the only place where we set PublishName (apart from factories etc), and we must ensure
            // that we do have an invariant name, as soon as we have a variant name, else we would end up not
            // being able to publish - and not being able to change the name, as PublishName is readonly.
            // see also: DocumentRepository.EnsureInvariantNameValues() - which deals with Name.
            // see also: U4-11286
            if (culture == null || string.IsNullOrEmpty(PublishName))
            {
                PublishName = name;
                PublishDate = date;
            }

            if (culture != null)
            {
                // private method, assume that culture is valid

                if (_publishInfos == null)
                    _publishInfos = new Dictionary<string, (string Name, DateTime Date)>(StringComparer.OrdinalIgnoreCase);

                _publishInfos[culture] = (name, date);
            }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IReadOnlyDictionary<string, string> PublishCultureNames => _publishInfos?.ToDictionary(x => x.Key, x => x.Value.Name, StringComparer.OrdinalIgnoreCase) ?? NoNames;

        /// <inheritdoc/>
        public string GetPublishName(string culture)
        {
            if (culture == null) return PublishName;
            if (_publishInfos == null) return null;
            return _publishInfos.TryGetValue(culture, out var infos) ? infos.Name : null;
        }

        // clears a publish name
        private void ClearPublishName(string culture)
        {
            if (culture == null)
            {
                PublishName = null;
                return;
            }

            if (_publishInfos == null) return;
            _publishInfos.Remove(culture);
            if (_publishInfos.Count == 0)
                _publishInfos = null;
        }

        // clears all publish names
        private void ClearPublishNames()
        {
            PublishName = null;
            _publishInfos = null;
        }

        /// <inheritdoc />
        public bool IsCulturePublished(string culture)
            => !string.IsNullOrWhiteSpace(GetPublishName(culture));

        /// <inheritdoc />
        public DateTime GetCulturePublishDate(string culture)
        {
            if (_publishInfos != null && _publishInfos.TryGetValue(culture, out var infos))
                return infos.Date;
            throw new InvalidOperationException($"Culture \"{culture}\" is not published.");
        }

        /// <inheritdoc />
        public IEnumerable<string> PublishedCultures => _publishInfos?.Keys ?? Enumerable.Empty<string>();

        /// <inheritdoc />
        public bool IsCultureEdited(string culture)
        {
            return string.IsNullOrWhiteSpace(GetPublishName(culture)) || (_edited != null && _edited.Contains(culture));
        }

        // sets a publish edited
        internal void SetCultureEdited(string culture)
        {
            if (_edited == null)
                _edited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _edited.Add(culture);
        }

        // sets all publish edited
        internal void SetCultureEdited(IEnumerable<string> cultures)
        {
            _edited = new HashSet<string>(cultures, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public IEnumerable<string> EditedCultures => CultureNames.Keys.Where(IsCultureEdited);

        /// <inheritdoc />
        public IEnumerable<string> AvailableCultures => CultureNames.Keys;

        [IgnoreDataMember]
        public int PublishedVersionId { get; internal set; }

        [DataMember]
        public bool Blueprint { get; internal set; }

        /// <inheritdoc />
        internal virtual bool TryPublishAllValues()
        {
            // the values we want to publish should be valid
            if (ValidateAllProperties().Any())
                return false; //fixme this should return an attempt with error results

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException($"Cannot publish invariant culture without a name.");
            PublishName = Name;
            var now = DateTime.Now;
            foreach (var (culture, name) in CultureNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return false; //fixme this should return an attempt with error results

                SetPublishInfos(culture, name, now);
            }

            // property.PublishAllValues only deals with supported variations (if any)
            foreach (var property in Properties)
                property.PublishAllValues();

            _publishedState = PublishedState.Publishing;
            return true;
        }

        /// <inheritdoc />
        public virtual bool TryPublishValues(string culture = null, string segment = null)
        {
            // the variation should be supported by the content type
            ContentType.ValidateVariation(culture, segment, throwIfInvalid: true);

            // the values we want to publish should be valid
            if (ValidateProperties(culture, segment).Any())
                return false; //fixme this should return an attempt with error results

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            if (segment == null)
            {
                var name = GetName(culture);
                if (string.IsNullOrWhiteSpace(name))
                    return false; //fixme this should return an attempt with error results

                SetPublishInfos(culture, name, DateTime.Now);
            }

            // property.PublishValue throws on invalid variation, so filter them out
            foreach (var property in Properties.Where(x => x.PropertyType.ValidateVariation(culture, segment, throwIfInvalid: false)))
                property.PublishValue(culture, segment);

            _publishedState = PublishedState.Publishing;
            return true;
        }

        /// <inheritdoc />
        internal virtual bool PublishCultureValues(string culture = null)
        {
            //fixme - needs API review as this is not used apart from in tests

            // the values we want to publish should be valid
            if (ValidatePropertiesForCulture(culture).Any())
                return false;

            // Name and PublishName are managed by the repository, but Names and PublishNames
            // must be managed here as they depend on the existing / supported variations.
            var name = GetName(culture);
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"Cannot publish {culture ?? "invariant"} culture without a name.");
            SetPublishInfos(culture, name, DateTime.Now);

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
            foreach (var (culture, name) in other.CultureNames)
                SetName(name, culture);
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
            SetName(other.GetName(culture), culture);
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
            SetName(other.GetName(culture), culture);
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
