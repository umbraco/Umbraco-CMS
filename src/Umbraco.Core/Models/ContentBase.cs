using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
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
        protected static readonly Dictionary<string, string> NoNames = new Dictionary<string, string>();
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private int _contentTypeId;
        protected IContentTypeComposition ContentTypeBase;
        private int _writerId;
        private PropertyCollection _properties;
        private Dictionary<string, string> _names;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBase"/> class.
        /// </summary>
        protected ContentBase(string name, int parentId, IContentTypeComposition contentType, PropertyCollection properties)
            : this(name, contentType, properties)
        {
            if (parentId == 0) throw new ArgumentOutOfRangeException(nameof(parentId));
            ParentId = parentId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBase"/> class.
        /// </summary>
        protected ContentBase(string name, IContentBase parent, IContentTypeComposition contentType, PropertyCollection properties)
            : this(name, contentType, properties)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            SetParent(parent);
        }

        private ContentBase(string name, IContentTypeComposition contentType, PropertyCollection properties)
        {
            ContentTypeBase = contentType ?? throw new ArgumentNullException(nameof(contentType));

            // initially, all new instances have
            Id = 0; // no identity
            VersionId = 0; // no versions

            Name = name;
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
            public readonly PropertyInfo NamesSelector = ExpressionHelper.GetPropertyInfo<ContentBase, IReadOnlyDictionary<string, string>>(x => x.Names);
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
        [DataMember]
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
        /// fixme is a proxy, kill this
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> PropertyGroups => ContentTypeBase.CompositionPropertyGroups;

        /// <summary>
        /// Gets the numeration of property types for the entity.
        /// fixme is a proxy, kill this
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> PropertyTypes => ContentTypeBase.CompositionPropertyTypes;

        #region Cultures

        /// <inheritdoc />
        [DataMember]
        public virtual IReadOnlyDictionary<string, string> Names
        {
            get => _names ?? NoNames;
            set
            {
                foreach (var (culture, name) in value)
                    SetName(culture, name);
            }
        }

        /// <inheritdoc />
        public virtual void SetName(string culture, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ClearName(culture);
                return;
            }

            if (culture == null)
            {
                Name = name;
                return;
            }

            if (!ContentTypeBase.Variations.HasAny(ContentVariation.CultureNeutral | ContentVariation.CultureSegment))
                throw new NotSupportedException("Content type does not support varying name by culture.");

            if (_names == null)
                _names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            _names[culture] = name;
            OnPropertyChanged(Ps.Value.NamesSelector);
        }

        /// <inheritdoc />
        public virtual string GetName(string culture)
        {
            if (culture == null) return Name;
            if (_names == null) return null;
            return _names.TryGetValue(culture, out var name) ? name : null;
        }

        /// <inheritdoc />
        public bool IsCultureAvailable(string culture)
            => !string.IsNullOrWhiteSpace(GetName(culture));

        private void ClearName(string culture)
        {
            if (culture == null)
            {
                Name = null;
                return;
            }

            if (_names == null) return;
            _names.Remove(culture);
            if (_names.Count == 0)
                _names = null;
        }

        protected virtual void ClearNames()
        {
            _names = null;
            OnPropertyChanged(Ps.Value.NamesSelector);
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

        // HttpPostedFileBase is the base class that can be mocked
        // HttpPostedFile is what we get in ASP.NET
        // HttpPostedFileWrapper wraps sealed HttpPostedFile as HttpPostedFileBase

        /// <summary>
        /// Sets the posted file value of a property.
        /// </summary>
        public virtual void SetValue(string propertyTypeAlias, HttpPostedFile value, string culture = null, string segment = null)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, new HttpPostedFileWrapper(value), culture, segment);
        }

        /// <summary>
        /// Sets the posted file value of a property.
        /// </summary>
        public virtual void SetValue(string propertyTypeAlias, HttpPostedFileBase value, string culture = null, string segment = null)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value, culture, segment);
        }

        #endregion

        #region Validation

        public virtual Property[] ValidateAll()
        {
            return Properties.Where(x => !x.IsAllValid()).ToArray();
        }

        public virtual Property[] Validate(string culture = null, string segment = null)
        {
            return Properties.Where(x => !x.IsValid(culture, segment)).ToArray();
        }

        public virtual Property[] ValidateCulture(string culture = null)
        {
            return Properties.Where(x => !x.IsCultureValid(culture)).ToArray();
        }

        #endregion

        #region Dirty

        /// <inheritdoc />
        /// <remarks>Overriden to include user properties.</remarks>
        public override void ResetDirtyProperties(bool rememberDirty)
        {
            base.ResetDirtyProperties(rememberDirty);

            // also reset dirty changes made to user's properties
            foreach (var prop in Properties)
                prop.ResetDirtyProperties(rememberDirty);
        }

        /// <inheritdoc />
        /// <remarks>Overriden to include user properties.</remarks>
        public override bool IsDirty()
        {
            return IsEntityDirty() || this.IsAnyUserPropertyDirty();
        }

        /// <inheritdoc />
        /// <remarks>Overriden to include user properties.</remarks>
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
        /// <remarks>Overriden to include user properties.</remarks>
        public override bool IsPropertyDirty(string propertyName)
        {
            if (base.IsPropertyDirty(propertyName))
                return true;

            return Properties.Contains(propertyName) && Properties[propertyName].IsDirty();
        }

        /// <inheritdoc />
        /// <remarks>Overriden to include user properties.</remarks>
        public override bool WasPropertyDirty(string propertyName)
        {
            if (base.WasPropertyDirty(propertyName))
                return true;

            return Properties.Contains(propertyName) && Properties[propertyName].WasDirty();
        }

        /// <inheritdoc />
        /// <remarks>Overriden to include user properties.</remarks>
        public override IEnumerable<string> GetDirtyProperties()
        {
            var instanceProperties = base.GetDirtyProperties();
            var propertyTypes = Properties.Where(x => x.IsDirty()).Select(x => x.Alias);
            return instanceProperties.Concat(propertyTypes);
        }

        /// <inheritdoc />
        /// <remarks>Overriden to include user properties.</remarks>
        public override IEnumerable<string> GetWereDirtyProperties()
        {
            var instanceProperties = base.GetWereDirtyProperties();
            var propertyTypes = Properties.Where(x => x.WasDirty()).Select(x => x.Alias);
            return instanceProperties.Concat(propertyTypes);
        }

        #endregion
    }
}
