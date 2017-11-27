using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for base Content properties and methods
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, ContentType: {ContentTypeBase.Alias}")]
    public abstract class ContentBase : Entity, IContentBase
    {
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private int _contentTypeId;
        protected IContentTypeComposition ContentTypeBase;

        private Lazy<int> _parentId;
        private int _level;
        private string _path;
        private int _sortOrder;

        private bool _trashed;
        private int _creatorId;
        private int _writerId;

        // fixme need to deal with localized names, how?
        // for the time being, this is the node text = unique
        private string _name;

        private PropertyCollection _properties;
        private readonly List<Property> _invalidProperties = new List<Property>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        IDictionary<string, object> IUmbracoEntity.AdditionalData => _lazyAdditionalData.Value;
        private readonly Lazy<Dictionary<string, object>> _lazyAdditionalData = new Lazy<Dictionary<string, object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBase"/> class.
        /// </summary>
        protected ContentBase(string name, int parentId, IContentTypeComposition contentType, PropertyCollection properties)
            : this(name, contentType, properties)
        {
            if (parentId == 0) throw new ArgumentOutOfRangeException(nameof(parentId));
            _parentId = new Lazy<int>(() => parentId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBase"/> class.
        /// </summary>
        protected ContentBase(string name, IContentBase parent, IContentTypeComposition contentType, PropertyCollection properties)
            : this(name, contentType, properties)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _parentId = new Lazy<int>(() => parent.Id);
        }

        private ContentBase(string name, IContentTypeComposition contentType, PropertyCollection properties)
        {
            ContentTypeBase = contentType ?? throw new ArgumentNullException(nameof(contentType));

            // initially, all new instances have
            Id = 0; // no identity
            Version = Guid.NewGuid(); // a new unique version id

            _name = name;
            _contentTypeId = contentType.Id;
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _properties.EnsurePropertyTypes(PropertyTypes);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ParentId);
            public readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.Level);
            public readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Path);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.SortOrder);
            public readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<ContentBase, bool>(x => x.Trashed);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Name);
            public readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.CreatorId);
            public readonly PropertyInfo DefaultContentTypeIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ContentTypeId);
            public readonly PropertyInfo PropertyCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentBase, PropertyCollection>(x => x.Properties);
            public readonly PropertyInfo WriterSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.WriterId);
        }

        protected void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the identifier of the parent entity.
        /// </summary>
        [DataMember]
        public virtual int ParentId
        {
            get
            {
                var val = _parentId.Value;
                if (val == 0)
                {
                    throw new InvalidOperationException("The ParentId cannot have a value of 0. Perhaps the parent object used to instantiate this object has not been persisted to the data store.");
                }
                return val;
            }
            set
            {
                _parentId = new Lazy<int>(() => value);
                OnPropertyChanged(Ps.Value.ParentIdSelector);
            }
        }

        /// <summary>
        /// Sets the identifier of the parent entity.
        /// </summary>
        /// <param name="parentId">Id of the Parent</param>
        protected internal void SetLazyParentId(Lazy<int> parentId)
        {
            _parentId = parentId;
            OnPropertyChanged(Ps.Value.ParentIdSelector);
        }

        /// <summary>
        /// Gets or sets the level of the entity.
        /// </summary>
        [DataMember]
        public virtual int Level
        {
            get => _level;
            set => SetPropertyValueAndDetectChanges(value, ref _level, Ps.Value.LevelSelector);
        }

        /// <summary>
        /// Gets or sets the path of the entity.
        /// </summary>
        [DataMember]
        public virtual string Path //Setting this value should be handled by the class not the user
        {
            get => _path;
            set => SetPropertyValueAndDetectChanges(value, ref _path, Ps.Value.PathSelector);
        }

        /// <summary>
        /// Gets or sets the sort order of the  entity.
        /// </summary>
        [DataMember]
        public virtual int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is trashed.
        /// </summary>
        /// <remarks>A trashed entity is unpublished and in the recycle bin.</remarks>
        [DataMember]
        public virtual bool Trashed // fixme setting this value should be handled by the class not the user
        {
            get => _trashed;
            internal set => SetPropertyValueAndDetectChanges(value, ref _trashed, Ps.Value.TrashedSelector);
        }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        [DataMember]
        public virtual string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector);
        }

        /// <summary>
        /// Gets or sets the identifier of the user who created the entity.
        /// </summary>
        [DataMember]
        public virtual int CreatorId
        {
            get => _creatorId;
            set => SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.CreatorIdSelector);
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

        /// <summary>
        /// Gets or sets the identifier of the version.
        /// </summary>
        [DataMember]
        public Guid Version { get; internal set; }

        [IgnoreDataMember]
        internal int VersionPk { get; set; }

        [IgnoreDataMember]
        internal int PublishedVersionPk { get; set; }

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
            protected set { SetPropertyValueAndDetectChanges(value, ref _contentTypeId, Ps.Value.DefaultContentTypeIdSelector); }
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

        #region Has, Get, Set, Publish Property Value

        /// <summary>
        /// Gets a value indicating whether the content entity has a property with the supplied alias.
        /// </summary>
        /// <remarks>Indicates that the content entity has a property with the supplied alias, but
        /// not necessarily that the content has a value for that property. Could be missing.</remarks>
        public virtual bool HasProperty(string propertyTypeAlias)
            => Properties.Contains(propertyTypeAlias);

        /// <summary>
        /// Gets the neutral value of a property.
        /// </summary>
        public virtual object GetValue(string propertyTypeAlias, bool published = false)
        {
            return Properties.TryGetValue(propertyTypeAlias, out var property)
                ? property.GetValue(published)
                : null;
        }

        /// <summary>
        /// Gets the culture value of a property.
        /// </summary>
        public virtual object GetValue(string propertyTypeAlias, int? languageId, bool published = false)
        {
            return Properties.TryGetValue(propertyTypeAlias, out var property)
                ? property.GetValue(languageId, null, published)
                : null;
        }

        /// <summary>
        /// Gets the segment value of a property.
        /// </summary>
        public virtual object GetValue(string propertyTypeAlias, string segment, bool published = false)
        {
            return Properties.TryGetValue(propertyTypeAlias, out var property)
                ? property.GetValue(null, segment, published)
                : null;
        }

        /// <summary>
        /// Gets the culture+segment value of a property.
        /// </summary>
        public virtual object GetValue(string propertyTypeAlias, int? languageId, string segment, bool published = false)
        {
            return Properties.TryGetValue(propertyTypeAlias, out var property)
                ? property.GetValue(languageId, segment, published)
                : null;
        }

        /// <summary>
        /// Gets the typed neutral value of a property.
        /// </summary>
        public virtual TPropertyValue GetValue<TPropertyValue>(string propertyTypeAlias, bool published = false)
        {
            if (!Properties.TryGetValue(propertyTypeAlias, out var property))
                return default;

            var convertAttempt = property.GetValue(published).TryConvertTo<TPropertyValue>();
            return convertAttempt.Success ? convertAttempt.Result : default;
        }

        /// <summary>
        /// Gets the typed culture value of a property.
        /// </summary>
        public virtual TPropertyValue GetValue<TPropertyValue>(string propertyTypeAlias, int? languageId, bool published = false)
        {
            if (!Properties.TryGetValue(propertyTypeAlias, out var property))
                return default;

            var convertAttempt = property.GetValue(languageId, null, published).TryConvertTo<TPropertyValue>();
            return convertAttempt.Success ? convertAttempt.Result : default;
        }

        /// <summary>
        /// Gets the typed segment value of a property.
        /// </summary>
        public virtual TPropertyValue GetValue<TPropertyValue>(string propertyTypeAlias, string segment, bool published = false)
        {
            if (!Properties.TryGetValue(propertyTypeAlias, out var property))
                return default;

            var convertAttempt = property.GetValue(null, segment, published).TryConvertTo<TPropertyValue>();
            return convertAttempt.Success ? convertAttempt.Result : default;
        }

        /// <summary>
        /// Gets the typed culture+segment value of a property.
        /// </summary>
        public virtual TPropertyValue GetValue<TPropertyValue>(string propertyTypeAlias, int? languageId, string segment, bool published = false)
        {
            if (!Properties.TryGetValue(propertyTypeAlias, out var property))
                return default;

            var convertAttempt = property.GetValue(languageId, segment, published).TryConvertTo<TPropertyValue>();
            return convertAttempt.Success ? convertAttempt.Result : default;
        }

        /// <summary>
        /// Sets the neutral (draft) value of a property.
        /// </summary>
        public virtual void SetValue(string propertyTypeAlias, object value)
        {
            SetValueOnProperty(propertyTypeAlias, null, null, value);
        }

        /// <summary>
        /// Sets the culture (draft) value of a property.
        /// </summary>
        public virtual void SetValue(string propertyTypeAlias, int? languageId, object value)
        {
            SetValueOnProperty(propertyTypeAlias, languageId, null, value);
        }

        /// <summary>
        /// Sets the culture+segment (draft) value of a property.
        /// </summary>
        public virtual void SetValue(string propertyTypeAlias, string segment, object value)
        {
            SetValueOnProperty(propertyTypeAlias, null, segment, value);
        }

        /// <summary>
        /// Sets the culture+segment (draft) value of a property.
        /// </summary>
        public virtual void SetValue(string propertyTypeAlias, int? languageId, string segment, object value)
        {
            SetValueOnProperty(propertyTypeAlias, languageId, segment, value);
        }

        // fixme - these three use an extension method that needs to be adapted too

        // HttpPostedFileBase is the base class that can be mocked
        // HttpPostedFile is what we get in ASP.NET
        // HttpPostedFileWrapper wraps sealed HttpPostedFile as HttpPostedFileBase

        /// <summary>
        /// Sets the posted file value of a Property
        /// </summary>
        public virtual void SetValue(string propertyTypeAlias, HttpPostedFile value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, new HttpPostedFileWrapper(value));
        }

        /// <summary>
        /// Sets the posted file base value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetValue(string propertyTypeAlias, HttpPostedFileBase value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the (edited) value of a property.
        /// </summary>
        private void SetValueOnProperty(string propertyTypeAlias, int? languageId, string segment, object value)
        {
            if (Properties.Contains(propertyTypeAlias))
            {
                Properties[propertyTypeAlias].SetValue(languageId, segment, value);
                return;
            }

            var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (propertyType == null)
                throw new InvalidOperationException($"No PropertyType exists with the supplied alias \"{propertyTypeAlias}\".");

            var property = propertyType.CreateProperty();
            property.SetValue(languageId, segment, value);
            Properties.Add(property);
        }

        #endregion

        #region Validation

        public virtual Property[] Validate()
        {
            return Properties.Where(x => !x.IsValid()).ToArray();
        }

        public virtual Property[] Validate(int? languageId)
        {
            return Properties.Where(x => !x.IsValid(languageId)).ToArray();
        }

        public virtual Property[] Validate(string segment)
        {
            return Properties.Where(x => !x.IsValid(segment)).ToArray();
        }

        public virtual Property[] Validate(int? languageId, string segment)
        {
            return Properties.Where(x => !x.IsValid(languageId, segment)).ToArray();
        }

        #endregion

        #region Dirty

        /// <summary>
        /// Resets dirty properties.
        /// </summary>
        public override void ResetDirtyProperties(bool rememberDirty)
        {
            base.ResetDirtyProperties(rememberDirty);

            // also reset dirty changes made to user's properties
            foreach (var prop in Properties)
                prop.ResetDirtyProperties(rememberDirty);
        }

        /// <summary>
        /// Gets a value indicating whether the current entity is dirty.
        /// </summary>
        public override bool IsDirty()
        {
            return IsEntityDirty() || this.IsAnyUserPropertyDirty();
        }

        /// <summary>
        /// Gets a value indicating whether the current entity was dirty.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether a user property is dirty.
        /// </summary>
        public override bool IsPropertyDirty(string propertyName)
        {
            if (base.IsPropertyDirty(propertyName))
                return true;

            return Properties.Contains(propertyName) && Properties[propertyName].IsDirty();
        }

        /// <summary>
        /// Gets a value indicating whether a user property was dirty.
        /// </summary>
        public override bool WasPropertyDirty(string propertyName)
        {
            if (base.WasPropertyDirty(propertyName))
                return true;

            return Properties.Contains(propertyName) && Properties[propertyName].WasDirty();
        }

        #endregion
    }
}
