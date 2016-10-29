﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using Umbraco.Core.Models.EntityBase;

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
        protected IContentTypeComposition ContentTypeBase;
        
        private Lazy<int> _parentId;
        private string _name;//NOTE Once localization is introduced this will be the localized Name of the Content/Media.
        private int _sortOrder;
        private int _level;
        private string _path;
        private int _creatorId;
        private bool _trashed;
        private int _contentTypeId;
        private PropertyCollection _properties;
        private readonly List<Property> _lastInvalidProperties = new List<Property>();

        /// <summary>
        /// Protected constructor for ContentBase (Base for Content and Media)
        /// </summary>
        /// <param name="name">Localized Name of the entity</param>
        /// <param name="parentId"></param>
        /// <param name="contentType"></param>
        /// <param name="properties"></param>
        protected ContentBase(string name, int parentId, IContentTypeComposition contentType, PropertyCollection properties)
        {
            Mandate.ParameterCondition(parentId != 0, "parentId");
            Mandate.ParameterNotNull(contentType, "contentType");
            Mandate.ParameterNotNull(properties, "properties");

            ContentTypeBase = contentType;
            Version = Guid.NewGuid();

            _parentId = new Lazy<int>(() => parentId);
            _name = name;
            _contentTypeId = contentType.Id;
            _properties = properties;
            _properties.EnsurePropertyTypes(PropertyTypes);
            _additionalData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Protected constructor for ContentBase (Base for Content and Media)
        /// </summary>
        /// <param name="name">Localized Name of the entity</param>
        /// <param name="parent"></param>
        /// <param name="contentType"></param>
        /// <param name="properties"></param>
        protected ContentBase(string name, IContentBase parent, IContentTypeComposition contentType, PropertyCollection properties)
		{
			Mandate.ParameterNotNull(parent, "parent");
			Mandate.ParameterNotNull(contentType, "contentType");
			Mandate.ParameterNotNull(properties, "properties");

            ContentTypeBase = contentType;
            Version = Guid.NewGuid();

			_parentId = new Lazy<int>(() => parent.Id);
            _name = name;
			_contentTypeId = contentType.Id;
			_properties = properties;
			_properties.EnsurePropertyTypes(PropertyTypes);
            _additionalData = new Dictionary<string, object>();
		}

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Name);
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ParentId);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.SortOrder);
            public readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.Level);
            public readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Path);
            public readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.CreatorId);
            public readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<ContentBase, bool>(x => x.Trashed);
            public readonly PropertyInfo DefaultContentTypeIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ContentTypeId);
            public readonly PropertyInfo PropertyCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentBase, PropertyCollection>(x => x.Properties);
        }        

        protected void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertyCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Id of the Parent entity
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
        /// Gets or sets the name of the entity
        /// </summary>
        [DataMember]
        public virtual string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        public virtual int SortOrder
        {
            get { return _sortOrder; }
            set { SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector); }
        }

        /// <summary>
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        public virtual int Level
        {
            get { return _level; }
            set { SetPropertyValueAndDetectChanges(value, ref _level, Ps.Value.LevelSelector); }
        }

        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [DataMember]
        public virtual string Path //Setting this value should be handled by the class not the user
        {
            get { return _path; }
            set { SetPropertyValueAndDetectChanges(value, ref _path, Ps.Value.PathSelector); }
        }

        /// <summary>
        /// Profile of the user who created this Content
        /// </summary>
        [DataMember]
        public virtual int CreatorId
        {
            get { return _creatorId; }
            set { SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.CreatorIdSelector); }
        }
        
        /// <summary>
        /// Boolean indicating whether this Content is Trashed or not.
        /// If Content is Trashed it will be located in the Recyclebin.
        /// </summary>
        /// <remarks>When content is trashed it should be unpublished</remarks>
        [DataMember]
        public virtual bool Trashed //Setting this value should be handled by the class not the user
        {
            get { return _trashed; }
            internal set { SetPropertyValueAndDetectChanges(value, ref _trashed, Ps.Value.TrashedSelector); }
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
        /// Collection of properties, which make up all the data available for this Content object
        /// </summary>
        [DataMember]
        public virtual PropertyCollection Properties
        {
            get { return _properties; }
            set
            {
                _properties = value;
                _properties.CollectionChanged += PropertiesChanged;
            }
        }

        private readonly IDictionary<string, object> _additionalData;

        /// <summary>
        /// Some entities may expose additional data that other's might not, this custom data will be available in this collection
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDictionary<string, object> IUmbracoEntity.AdditionalData
        {
            get { return _additionalData; }
        }

        /// <summary>
        /// List of PropertyGroups available on this Content object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> PropertyGroups { get { return ContentTypeBase.CompositionPropertyGroups; } }

        /// <summary>
        /// List of PropertyTypes available on this Content object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> PropertyTypes { get { return ContentTypeBase.CompositionPropertyTypes; } }

        /// <summary>
        /// Indicates whether the content object has a property with the supplied alias
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>True if Property with given alias exists, otherwise False</returns>
        public virtual bool HasProperty(string propertyTypeAlias)
        {
            return Properties.Contains(propertyTypeAlias);
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as an <see cref="object"/></returns>
        public virtual object GetValue(string propertyTypeAlias)
        {
            return Properties[propertyTypeAlias].Value;
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <typeparam name="TPassType">Type of the value to return</typeparam>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as a <see cref="TPassType"/></returns>
        public virtual TPassType GetValue<TPassType>(string propertyTypeAlias)
        {
            var convertAttempt = Properties[propertyTypeAlias].Value.TryConvertTo<TPassType>();
            return convertAttempt.Success ? convertAttempt.Result : default(TPassType);
        }

        /// <summary>
        /// Sets the <see cref="System.Object"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetValue(string propertyTypeAlias, object value)
        {
            if (value == null)
            {
                SetValueOnProperty(propertyTypeAlias, value);
                return;
            }

            // .NET magic to call one of the 'SetPropertyValue' handlers with matching signature 
            ((dynamic)this).SetPropertyValue(propertyTypeAlias, (dynamic)value);
        }

        /// <summary>
        /// Sets the <see cref="System.String"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, string value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Int32"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, int value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Int64"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, long value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Decimal"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, decimal value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Double"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, double value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Boolean"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, bool value)
        {
            int val = Convert.ToInt32(value);
            SetValueOnProperty(propertyTypeAlias, val);
        }

        /// <summary>
        /// Sets the <see cref="System.DateTime"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, DateTime value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Web.HttpPostedFile"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>        
        public virtual void SetPropertyValue(string propertyTypeAlias, HttpPostedFile value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Web.HttpPostedFileBase"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, HttpPostedFileBase value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Web.HttpPostedFileWrapper"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        [Obsolete("There is no reason for this overload since HttpPostedFileWrapper inherits from HttpPostedFileBase")]
        public virtual void SetPropertyValue(string propertyTypeAlias, HttpPostedFileWrapper value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value);
        }

        /// <summary>
        /// Private method to set the value of a property
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        private void SetValueOnProperty(string propertyTypeAlias, object value)
        {
            if (Properties.Contains(propertyTypeAlias))
            {
                Properties[propertyTypeAlias].Value = value;
                return;
            }

            var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (propertyType == null)
            {
                throw new Exception(String.Format("No PropertyType exists with the supplied alias: {0}", propertyTypeAlias));
            }
            Properties.Add(propertyType.CreatePropertyFromValue(value));
        }

        /// <summary>
        /// Boolean indicating whether the content and its properties are valid
        /// </summary>
        /// <returns>True if content is valid otherwise false</returns>
        public virtual bool IsValid()
        {
            _lastInvalidProperties.Clear();
            _lastInvalidProperties.AddRange(Properties.Where(property => property.IsValid() == false));
            return _lastInvalidProperties.Any() == false;
        }

        /// <summary>
        /// Returns a collection of the result of the last validation process, this collection contains all invalid properties.
        /// </summary>
        [IgnoreDataMember]
        internal IEnumerable<Property> LastInvalidProperties
        {
            get { return _lastInvalidProperties; }
        }

        public abstract void ChangeTrashedState(bool isTrashed, int parentId = -20);

        #region Dirty property handling

        /// <summary>
        /// We will override this method to ensure that when we reset the dirty properties that we 
        /// also reset the dirty changes made to the content's Properties (user defined)
        /// </summary>
        /// <param name="rememberPreviouslyChangedProperties"></param>
        public override void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            base.ResetDirtyProperties(rememberPreviouslyChangedProperties);

            foreach (var prop in Properties)
            {
                prop.ResetDirtyProperties(rememberPreviouslyChangedProperties);
            }
        }

        /// <summary>
        /// Indicates whether the current entity is dirty.
        /// </summary>
        /// <returns>True if entity is dirty, otherwise False</returns>
        public override bool IsDirty()
        {
            return IsEntityDirty() || this.IsAnyUserPropertyDirty();
        }

        /// <summary>
        /// Indicates whether the current entity was dirty.
        /// </summary>
        /// <returns>True if entity was dirty, otherwise False</returns>
        public override bool WasDirty()
        {
            return WasEntityDirty() || this.WasAnyUserPropertyDirty();
        }

        /// <summary>
        /// Returns true if only the entity properties are dirty
        /// </summary>
        /// <returns></returns>
        public bool IsEntityDirty()
        {
            return base.IsDirty();
        }

        /// <summary>
        /// Returns true if only the entity properties were dirty
        /// </summary>
        /// <returns></returns>
        public bool WasEntityDirty()
        {
            return base.WasDirty();
        }

        /// <summary>
        /// Indicates whether a specific property on the current <see cref="IContent"/> entity is dirty.
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>
        /// True if any of the class properties are dirty or 
        /// True if any of the user defined PropertyType properties are dirty based on their alias, 
        /// otherwise False
        /// </returns>
        public override bool IsPropertyDirty(string propertyName)
        {
            bool existsInEntity = base.IsPropertyDirty(propertyName);
            if (existsInEntity)
                return true;

            if (Properties.Contains(propertyName))
            {
                return Properties[propertyName].IsDirty();
            }

            return false;
        }

        /// <summary>
        /// Indicates whether a specific property on the current entity was changed and the changes were committed
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>
        /// True if any of the class properties are dirty or 
        /// True if any of the user defined PropertyType properties are dirty based on their alias, 
        /// otherwise False
        /// </returns>
        public override bool WasPropertyDirty(string propertyName)
        {
            bool existsInEntity = base.WasPropertyDirty(propertyName);
            if (existsInEntity)
                return true;

            if (Properties.Contains(propertyName))
            {
                return Properties[propertyName].WasDirty();
            }

            return false;
        }

        #endregion
    }
}