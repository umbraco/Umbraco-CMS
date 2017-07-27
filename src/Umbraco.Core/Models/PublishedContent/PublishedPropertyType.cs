using System;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Composing;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an <see cref="IPublishedProperty"/> type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedPropertyType"/> class are immutable, ie
    /// if the property type changes, then a new class needs to be created.</remarks>
    public class PublishedPropertyType
    {
        private readonly PropertyValueConverterCollection _converters;
        private readonly object _locker = new object();
        private volatile bool _initialized;
        private IPropertyValueConverter _converter;
        private PropertyCacheLevel _cacheLevel;

        private Type _modelClrType;
        private Type _clrType;

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="PublishedPropertyType"/> class within a <see cref="PublishedContentType"/>,
        /// with a <see cref="PropertyType"/>.
        /// </summary>
        /// <param name="contentType">The published content type.</param>
        /// <param name="propertyType">The property type.</param>
        /// <remarks>The new published property type belongs to the published content type and corresponds to the property type.</remarks>
        public PublishedPropertyType(PublishedContentType contentType, PropertyType propertyType)
        {
            // PropertyEditor [1:n] DataTypeDefinition [1:n] PropertyType

            _converters = Current.PropertyValueConverters; // fixme really?

            ContentType = contentType;
            PropertyTypeAlias = propertyType.Alias;

            DataTypeId = propertyType.DataTypeDefinitionId;
            PropertyEditorAlias = propertyType.PropertyEditorAlias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedPropertyType"/> class with a property type alias and a property editor alias.
        /// </summary>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="propertyEditorAlias">The property editor alias.</param>
        /// <remarks>
        /// <para>The new published property type does not belong to a published content type.</para>
        /// <para>It is based upon the property editor, but has no datatype definition. This will work as long
        /// as the datatype definition is not required to process (eg to convert) the property values. For
        /// example, this may not work if the related IPropertyValueConverter requires the datatype definition
        /// to make decisions, fetch prevalues, etc.</para>
        /// <para>The value of <paramref name="propertyEditorAlias"/> is assumed to be valid.</para>
        /// </remarks>
        internal PublishedPropertyType(string propertyTypeAlias, string propertyEditorAlias, PropertyValueConverterCollection converters = null)
            : this(propertyTypeAlias, 0, propertyEditorAlias, converters)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedPropertyType"/> class with a property type alias,
        /// a datatype definition identifier, and a property editor alias.
        /// </summary>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="dataTypeDefinitionId">The datatype definition identifier.</param>
        /// <param name="propertyEditorAlias">The property editor alias.</param>
        /// <param name="umbraco">A value indicating whether the property is an Umbraco-defined property.</param>
        /// <remarks>
        /// <para>The new published property type does not belong to a published content type.</para>
        /// <para>The values of <paramref name="dataTypeDefinitionId"/> and <paramref name="propertyEditorAlias"/> are
        /// assumed to be valid and consistent.</para>
        /// </remarks>
        internal PublishedPropertyType(string propertyTypeAlias, int dataTypeDefinitionId, string propertyEditorAlias, PropertyValueConverterCollection converters = null, bool umbraco = false)
        {
            // ContentType
            // - in unit tests, to be set by PublishedContentType when creating it

            _converters = converters ?? Current.PropertyValueConverters; // fixme really?

            PropertyTypeAlias = propertyTypeAlias;

            DataTypeId = dataTypeDefinitionId;
            PropertyEditorAlias = propertyEditorAlias;
            IsUmbraco = umbraco;
        }

        #endregion

        #region Property type

        /// <summary>
        /// Gets or sets the published content type containing the property type.
        /// </summary>
        // internally set by PublishedContentType constructor
        public PublishedContentType ContentType { get; internal set; }

        /// <summary>
        /// Gets or sets the alias uniquely identifying the property type.
        /// </summary>
        public string PropertyTypeAlias { get; }

        /// <summary>
        /// Gets or sets the identifier uniquely identifying the data type supporting the property type.
        /// </summary>
        public int DataTypeId { get; }

        /// <summary>
        /// Gets or sets the alias uniquely identifying the property editor for the property type.
        /// </summary>
        public string PropertyEditorAlias { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is an Umbraco-defined property.
        /// </summary>
        internal bool IsUmbraco { get; }

        #endregion

        #region Converters

        private void Initialize()
        {
            if (_initialized) return;
            lock (_locker)
            {
                if (_initialized) return;
                InitializeLocked();
                _initialized = true;
            }
        }

        private void InitializeLocked()
        {
            _converter = null;
            var isdefault = false;

            foreach (var converter in _converters)
            {
                if (converter.IsConverter(this) == false)
                    continue;

                if (_converter == null)
                {
                    _converter = converter;
                    isdefault = _converters.IsDefault(converter);
                    continue;
                }

                if (isdefault)
                {
                    if (_converters.IsDefault(converter))
                    {
                        // previous was default, and got another default
                        if (_converters.Shadows(_converter, converter))
                        {
                            // previous shadows, ignore
                        }
                        else if (_converters.Shadows(converter, _converter))
                        {
                            // shadows previous, replace
                            _converter = converter;
                        }
                        else
                        {
                            // no shadow - bad
                            throw new InvalidOperationException(string.Format("Type '{2}' cannot be an IPropertyValueConverter"
                                                                              + " for property '{1}' of content type '{0}' because type '{3}' has already been detected as a converter"
                                                                              + " for that property, and only one converter can exist for a property.",
                                ContentType.Alias, PropertyTypeAlias,
                                converter.GetType().FullName, _converter.GetType().FullName));
                        }
                    }
                    else
                    {
                        // previous was default, replaced by non-default
                        _converter = converter;
                        isdefault = false;
                    }
                }
                else
                {
                    if (_converters.IsDefault(converter))
                    {
                        // previous was non-default, ignore default
                    }
                    else
                    {
                        // previous was non-default, and got another non-default - bad
                        throw new InvalidOperationException(string.Format("Type '{2}' cannot be an IPropertyValueConverter"
                                                                          + " for property '{1}' of content type '{0}' because type '{3}' has already been detected as a converter"
                                                                          + " for that property, and only one converter can exist for a property.",
                            ContentType.Alias, PropertyTypeAlias,
                            converter.GetType().FullName, _converter.GetType().FullName));
                    }
                }
            }

            _cacheLevel = _converter?.GetPropertyCacheLevel(this) ?? PropertyCacheLevel.Facade;
            _modelClrType = _converter == null ? typeof (object) : _converter.GetPropertyValueType(this);
        }

        // gets the cache level
        public PropertyCacheLevel CacheLevel
        {
            get
            {
                if (!_initialized) Initialize();
                return _cacheLevel;
            }
        }

        // converts the source value into the inter value
        // uses converters, else falls back to dark (& performance-wise expensive) magic
        // source: the property source value
        // preview: whether we are previewing or not
        public object ConvertSourceToInter(IPropertySet owner, object source, bool preview)
        {
            if (!_initialized) Initialize();

            // use the converter if any, else just return the source value
            return _converter != null
                ? _converter.ConvertSourceToInter(owner, this, source, preview)
                : source;
        }

        // converts the inter value into the clr value
        // uses converters, else returns the inter value
        // inter: the property inter value
        // preview: whether we are previewing or not
        public object ConvertInterToObject(IPropertySet owner, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (!_initialized) Initialize();

            // use the converter if any, else just return the inter value
            return _converter != null
                ? _converter.ConvertInterToObject(owner, this, referenceCacheLevel, inter, preview)
                : inter;
        }

        // converts the inter value into the xpath value
        // uses the converter else returns the inter value as a string
        // if successful, returns either a string or an XPathNavigator
        // inter: the property inter value
        // preview: whether we are previewing or not
        public object ConvertInterToXPath(IPropertySet owner, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (!_initialized) Initialize();

            // use the converter if any
            if (_converter != null)
                return _converter.ConvertInterToXPath(owner, this, referenceCacheLevel, inter, preview);

            // else just return the inter value as a string or an XPathNavigator
            if (inter == null) return null;
            var xElement = inter as XElement;
            if (xElement != null)
                return xElement.CreateNavigator();
            return inter.ToString().Trim();
        }

        // gets the property CLR type
        // may contain some ModelType types
        public Type ModelClrType
        {
            get
            {
                if (!_initialized) Initialize();
                return _modelClrType;
            }
        }

        // gets the property CLR type
        // with mapped ModelType types (may throw)
        // fixme - inject the factory?
        public Type ClrType
        {
            get
            {
                if (!_initialized) Initialize();
                return _clrType ?? (_clrType = ModelType.Map(_modelClrType, Current.PublishedContentModelFactory.ModelTypeMap));
            }
        }

        #endregion
    }
}
