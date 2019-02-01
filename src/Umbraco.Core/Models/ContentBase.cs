using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for base Content properties and methods
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, ContentType: {ContentTypeBase.Alias}")]
    public abstract class ContentBase : TreeEntityBase, IContentBase
    {
        protected static readonly ContentCultureInfosCollection NoInfos = new ContentCultureInfosCollection();
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private int _contentTypeId;
        protected IContentTypeComposition ContentTypeBase;
        private int _writerId;
        private PropertyCollection _properties;
        private ContentCultureInfosCollection _cultureInfos;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBase"/> class.
        /// </summary>
        protected ContentBase(string name, int parentId, IContentTypeComposition contentType, PropertyCollection properties, string culture = null)
            : this(name, contentType, properties, culture)
        {
            if (parentId == 0) throw new ArgumentOutOfRangeException(nameof(parentId));
            ParentId = parentId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBase"/> class.
        /// </summary>
        protected ContentBase(string name, IContentBase parent, IContentTypeComposition contentType, PropertyCollection properties, string culture = null)
            : this(name, contentType, properties, culture)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            SetParent(parent);
        }

        private ContentBase(string name, IContentTypeComposition contentType, PropertyCollection properties, string culture = null)
        {
            ContentTypeBase = contentType ?? throw new ArgumentNullException(nameof(contentType));

            // initially, all new instances have
            Id = 0; // no identity
            VersionId = 0; // no versions

            SetCultureName(name, culture);

            _contentTypeId = contentType.Id;
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _properties.EnsurePropertyTypes(PropertyTypes);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo DefaultContentTypeIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ContentTypeId);
            public readonly PropertyInfo PropertyCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentBase, PropertyCollection>(x => x.Properties);
            public readonly PropertyInfo WriterSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.WriterId);
            public readonly PropertyInfo CultureInfosSelector = ExpressionHelper.GetPropertyInfo<ContentBase, IReadOnlyDictionary<string, ContentCultureInfos>>(x => x.CultureInfos);
        }

        protected void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyCollectionSelector);
        }

        /// <summary>
        /// Id of the user who wrote/updated this entity
        /// </summary>
        [DataMember]
        public virtual int WriterId
        {
            get => _writerId;
            set => SetPropertyValueAndDetectChanges(value, ref _writerId, Ps.Value.WriterSelector);
        }

        [IgnoreDataMember]
        public int VersionId { get; internal set; }

        /// <summary>
        /// Integer Id of the default ContentType
        /// </summary>
        [DataMember]
        public virtual int ContentTypeId
        {
            get
            {
                //There will be cases where this has not been updated to reflect the true content type ID.
                //This will occur when inserting new content.
                if (_contentTypeId == 0 && ContentTypeBase != null && ContentTypeBase.HasIdentity)
                {
                    _contentTypeId = ContentTypeBase.Id;
                }
                return _contentTypeId;
            }
            protected set => SetPropertyValueAndDetectChanges(value, ref _contentTypeId, Ps.Value.DefaultContentTypeIdSelector);
        }

        /// <summary>
        /// Gets or sets the collection of properties for the entity.
        /// </summary>
        /// <remarks>
        /// Marked DoNotClone since we'll manually clone the underlying field to deal with the event handling
        /// </remarks>
        [DataMember]
        [DoNotClone]
        public virtual PropertyCollection Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                _properties.CollectionChanged += PropertiesChanged;
            }
        }

        /// <summary>
        /// Gets the enumeration of property groups for the entity.
        /// TODO: remove this proxy method
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> PropertyGroups => ContentTypeBase.CompositionPropertyGroups;

        /// <summary>
        /// Gets the numeration of property types for the entity.
        /// TODO: remove this proxy method
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> PropertyTypes => ContentTypeBase.CompositionPropertyTypes;

        #region Cultures

        // notes - common rules
        // - setting a variant value on an invariant content type throws
        // - getting a variant value on an invariant content type returns null
        // - setting and getting the invariant value is always possible
        // - setting a null value clears the value

        /// <inheritdoc />
        public IEnumerable<string> AvailableCultures
            => _cultureInfos?.Keys ?? Enumerable.Empty<string>();

        /// <inheritdoc />
        public bool IsCultureAvailable(string culture)
            => _cultureInfos != null && _cultureInfos.ContainsKey(culture);

        /// <inheritdoc />
        [DataMember]
        public virtual IReadOnlyDictionary<string, ContentCultureInfos> CultureInfos => _cultureInfos ?? NoInfos;

        /// <inheritdoc />
        public string GetCultureName(string culture)
        {
            if (culture.IsNullOrWhiteSpace()) return Name;
            if (!ContentTypeBase.VariesByCulture()) return null;
            if (_cultureInfos == null) return null;
            return _cultureInfos.TryGetValue(culture, out var infos) ? infos.Name : null;
        }

        /// <inheritdoc />
        public DateTime? GetUpdateDate(string culture)
        {
            if (culture.IsNullOrWhiteSpace()) return null;
            if (!ContentTypeBase.VariesByCulture()) return null;
            if (_cultureInfos == null) return null;
            return _cultureInfos.TryGetValue(culture, out var infos) ? infos.Date : (DateTime?) null;
        }

        /// <inheritdoc />
        public void SetCultureName(string name, string culture)
        {
            if (ContentTypeBase.VariesByCulture()) // set on variant content type
            {
                if (culture.IsNullOrWhiteSpace()) // invariant is ok
                {
                    Name = name; // may be null
                }
                else if (name.IsNullOrWhiteSpace()) // clear
                {
                    ClearCultureInfo(culture);
                }
                else // set
                {
                    SetCultureInfo(culture, name, DateTime.Now);
                }
            }
            else // set on invariant content type
            {
                if (!culture.IsNullOrWhiteSpace()) // invariant is NOT ok
                    throw new NotSupportedException("Content type does not vary by culture.");

                Name = name; // may be null
            }
        }

        protected void ClearCultureInfos()
        {
            _cultureInfos?.Clear();
            _cultureInfos = null;
        }

        protected void ClearCultureInfo(string culture)
        {
            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            if (_cultureInfos == null) return;
            _cultureInfos.Remove(culture);
            if (_cultureInfos.Count == 0)
                _cultureInfos = null;
        }

        protected void TouchCultureInfo(string culture)
        {
            if (_cultureInfos == null || !_cultureInfos.TryGetValue(culture, out var infos)) return;
            _cultureInfos.AddOrUpdate(culture, infos.Name, DateTime.Now);
        }

        // internal for repository
        internal void SetCultureInfo(string culture, string name, DateTime date)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(name));

            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            if (_cultureInfos == null)
            {
                _cultureInfos = new ContentCultureInfosCollection();
                _cultureInfos.CollectionChanged += CultureInfosCollectionChanged;
            }

            _cultureInfos.AddOrUpdate(culture, name, date);
        }

        /// <summary>
        /// Handles culture infos collection changes.
        /// </summary>
        private void CultureInfosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.CultureInfosSelector);
        }

        #endregion

        #region Has, Get, Set, Publish Property Value

        /// <inheritdoc />
        public virtual bool HasProperty(string propertyTypeAlias)
            => Properties.Contains(propertyTypeAlias);

        /// <inheritdoc />
        public virtual object GetValue(string propertyTypeAlias, string culture = null, string segment = null, bool published = false)
        {
            return Properties.TryGetValue(propertyTypeAlias, out var property)
                ? property.GetValue(culture, segment, published)
                : null;
        }

        /// <inheritdoc />
        public virtual TValue GetValue<TValue>(string propertyTypeAlias, string culture = null, string segment = null, bool published = false)
        {
            if (!Properties.TryGetValue(propertyTypeAlias, out var property))
                return default;

            var convertAttempt = property.GetValue(culture, segment, published).TryConvertTo<TValue>();
            return convertAttempt.Success ? convertAttempt.Result : default;
        }

        /// <inheritdoc />
        public virtual void SetValue(string propertyTypeAlias, object value, string culture = null, string segment = null)
        {
            if (Properties.Contains(propertyTypeAlias))
            {
                Properties[propertyTypeAlias].SetValue(value, culture, segment);
                return;
            }

            var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (propertyType == null)
                throw new InvalidOperationException($"No PropertyType exists with the supplied alias \"{propertyTypeAlias}\".");

            var property = propertyType.CreateProperty();
            property.SetValue(value, culture, segment);
            Properties.Add(property);
        }

        #endregion

        #region Copy

        /// <inheritdoc />
        public virtual void CopyFrom(IContent other, string culture = "*")
        {
            if (other.ContentTypeId != ContentTypeId)
                throw new InvalidOperationException("Cannot copy values from a different content type.");

            culture = culture?.ToLowerInvariant().NullOrWhiteSpaceAsNull();

            // the variation should be supported by the content type properties
            //  if the content type is invariant, only '*' and 'null' is ok
            //  if the content type varies, everything is ok because some properties may be invariant
            if (!ContentTypeBase.SupportsPropertyVariation(culture, "*", true))
                throw new NotSupportedException($"Culture \"{culture}\" is not supported by content type \"{ContentTypeBase.Alias}\" with variation \"{ContentTypeBase.Variations}\".");

            // copying from the same Id and VersionPk
            var copyingFromSelf = Id == other.Id && VersionId == other.VersionId;
            var published = copyingFromSelf;

            // note: use property.SetValue(), don't assign pvalue.EditValue, else change tracking fails

            // clear all existing properties for the specified culture
            foreach (var property in Properties)
            {
                // each property type may or may not support the variation
                if (!property.PropertyType.SupportsVariation(culture, "*", wildcards: true))
                    continue;

                foreach (var pvalue in property.Values)
                    if (property.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment, wildcards: true) &&
                        (culture == "*" || pvalue.Culture.InvariantEquals(culture)))
                    {
                        property.SetValue(null, pvalue.Culture, pvalue.Segment);
                    }
            }

            // copy properties from 'other'
            var otherProperties = other.Properties;
            foreach (var otherProperty in otherProperties)
            {
                if (!otherProperty.PropertyType.SupportsVariation(culture, "*", wildcards: true))
                    continue;

                var alias = otherProperty.PropertyType.Alias;
                foreach (var pvalue in otherProperty.Values)
                {
                    if (otherProperty.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment, wildcards: true) &&
                        (culture == "*" || pvalue.Culture.InvariantEquals(culture)))
                    {
                        var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                        SetValue(alias, value, pvalue.Culture, pvalue.Segment);
                    }
                }
            }

            // copy names, too

            if (culture == "*")
                ClearCultureInfos();

            if (culture == null || culture == "*")
                Name = other.Name;

            foreach (var (otherCulture, otherInfos) in other.CultureInfos)
            {
                if (culture == "*" || culture == otherCulture)
                    SetCultureName(otherInfos.Name, otherCulture);
            }
        }

        #endregion

        #region Validation

        /// <inheritdoc />
        public virtual Property[] ValidateProperties(string culture = "*")
        {
            var alsoInvariant = culture != null && culture != "*";

            return Properties.Where(x => // select properties...
                    x.PropertyType.SupportsVariation(culture, "*", true) && // that support the variation
                    (!x.IsValid(culture) || (alsoInvariant && !x.IsValid(null)))) // and are not valid
                .ToArray();
        }

        #endregion

        #region Dirty

        /// <inheritdoc />
        /// <remarks>Overridden to include user properties.</remarks>
        public override void ResetDirtyProperties(bool rememberDirty)
        {
            base.ResetDirtyProperties(rememberDirty);

            // also reset dirty changes made to user's properties
            foreach (var prop in Properties)
                prop.ResetDirtyProperties(rememberDirty);

            // take care of culture infos
            if (_cultureInfos == null) return;

            foreach (var cultureInfo in _cultureInfos)
                cultureInfo.ResetDirtyProperties(rememberDirty);
        }

        /// <inheritdoc />
        /// <remarks>Overridden to include user properties.</remarks>
        public override bool IsDirty()
        {
            return IsEntityDirty() || this.IsAnyUserPropertyDirty();
        }

        /// <inheritdoc />
        /// <remarks>Overridden to include user properties.</remarks>
        public override bool WasDirty()
        {
            return WasEntityDirty() || this.WasAnyUserPropertyDirty();
        }

        /// <summary>
        /// Gets a value indicating whether the current entity's own properties (not user) are dirty.
        /// </summary>
        public bool IsEntityDirty()
        {
            return base.IsDirty();
        }

        /// <summary>
        /// Gets a value indicating whether the current entity's own properties (not user) were dirty.
        /// </summary>
        public bool WasEntityDirty()
        {
            return base.WasDirty();
        }

        /// <inheritdoc />
        /// <remarks>Overridden to include user properties.</remarks>
        public override bool IsPropertyDirty(string propertyName)
        {
            if (base.IsPropertyDirty(propertyName))
                return true;

            return Properties.Contains(propertyName) && Properties[propertyName].IsDirty();
        }

        /// <inheritdoc />
        /// <remarks>Overridden to include user properties.</remarks>
        public override bool WasPropertyDirty(string propertyName)
        {
            if (base.WasPropertyDirty(propertyName))
                return true;

            return Properties.Contains(propertyName) && Properties[propertyName].WasDirty();
        }

        /// <inheritdoc />
        /// <remarks>Overridden to include user properties.</remarks>
        public override IEnumerable<string> GetDirtyProperties()
        {
            var instanceProperties = base.GetDirtyProperties();
            var propertyTypes = Properties.Where(x => x.IsDirty()).Select(x => x.Alias);
            return instanceProperties.Concat(propertyTypes);
        }

        /// <inheritdoc />
        /// <remarks>Overridden to include user properties.</remarks>
        public override IEnumerable<string> GetWereDirtyProperties()
        {
            var instanceProperties = base.GetWereDirtyProperties();
            var propertyTypes = Properties.Where(x => x.WasDirty()).Select(x => x.Alias);
            return instanceProperties.Concat(propertyTypes);
        }

        #endregion

        /// <inheritdoc />
        /// <remarks>
        /// Overridden to deal with specific object instances
        /// </remarks>
        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedContent = (ContentBase)clone;

            //if culture infos exist then deal with event bindings
            if (clonedContent._cultureInfos != null)
            {
                clonedContent._cultureInfos.CollectionChanged -= CultureInfosCollectionChanged;          //clear this event handler if any
                clonedContent._cultureInfos = (ContentCultureInfosCollection) _cultureInfos.DeepClone(); //manually deep clone
                clonedContent._cultureInfos.CollectionChanged += clonedContent.CultureInfosCollectionChanged;    //re-assign correct event handler
            }

            //if properties exist then deal with event bindings
            if (clonedContent._properties != null)
            {
                clonedContent._properties.CollectionChanged -= PropertiesChanged;         //clear this event handler if any
                clonedContent._properties = (PropertyCollection) _properties.DeepClone(); //manually deep clone
                clonedContent._properties.CollectionChanged += clonedContent.PropertiesChanged;   //re-assign correct event handler
            }
        }
    }
}
