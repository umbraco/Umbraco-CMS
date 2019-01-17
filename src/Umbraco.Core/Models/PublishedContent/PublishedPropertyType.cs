using System;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents a published property type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedPropertyType"/> class are immutable, ie
    /// if the property type changes, then a new class needs to be created.</remarks>
    public class PublishedPropertyType
    {
        //todo - API design review, should this be an interface?

        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly PropertyValueConverterCollection _propertyValueConverters;
        private readonly object _locker = new object();
        private volatile bool _initialized;
        private IPropertyValueConverter _converter;
        private PropertyCacheLevel _cacheLevel;

        private Type _modelClrType;
        private Type _clrType;

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="PublishedPropertyType"/> class with a property type.
        /// </summary>
        /// <remarks>
        /// <para>The new published property type belongs to the published content type.</para>
        /// </remarks>
        public PublishedPropertyType(PublishedContentType contentType, PropertyType propertyType, PropertyValueConverterCollection propertyValueConverters, IPublishedModelFactory publishedModelFactory, IPublishedContentTypeFactory factory)
            : this(propertyType.Alias, propertyType.DataTypeId, true, propertyType.Variations, propertyValueConverters, publishedModelFactory, factory)
        {
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        }

        /// <summary>
        /// This constructor is for tests and is not intended to be used directly from application code.
        /// </summary>
        /// <remarks>
        /// <para>Values are assumed to be consisted and are not checked.</para>
        /// <para>The new published property type belongs to the published content type.</para>
        /// </remarks>
        public PublishedPropertyType(PublishedContentType contentType, string propertyTypeAlias, int dataTypeId, bool isUserProperty, ContentVariation variations, PropertyValueConverterCollection propertyValueConverters, IPublishedModelFactory publishedModelFactory, IPublishedContentTypeFactory factory)
            : this(propertyTypeAlias, dataTypeId, isUserProperty, variations, propertyValueConverters, publishedModelFactory, factory)
        {
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        }

        /// <summary>
        /// This constructor is for tests and is not intended to be used directly from application code.
        /// </summary>
        /// <remarks>
        /// <para>Values are assumed to be consistent and are not checked.</para>
        /// <para>The new published property type does not belong to a published content type.</para>
        /// </remarks>
        public PublishedPropertyType(string propertyTypeAlias, int dataTypeId, bool isUserProperty, ContentVariation variations, PropertyValueConverterCollection propertyValueConverters, IPublishedModelFactory publishedModelFactory, IPublishedContentTypeFactory factory)
        {
            _publishedModelFactory = publishedModelFactory ?? throw new ArgumentNullException(nameof(publishedModelFactory));
            _propertyValueConverters = propertyValueConverters ?? throw new ArgumentNullException(nameof(propertyValueConverters));

            Alias = propertyTypeAlias;

            IsUserProperty = isUserProperty;
            Variations = variations;

            DataType = factory.GetDataType(dataTypeId);
        }

        #endregion

        #region Property type

        /// <summary>
        /// Gets the published content type containing the property type.
        /// </summary>
        public PublishedContentType ContentType { get; internal set; } // internally set by PublishedContentType constructor

        /// <summary>
        /// Gets the data type.
        /// </summary>
        public PublishedDataType DataType { get; }

        /// <summary>
        /// Gets property type alias.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the property editor alias.
        /// </summary>
        public string EditorAlias => DataType.EditorAlias;

        /// <summary>
        /// Gets a value indicating whether the property is a user content property.
        /// </summary>
        /// <remarks>A non-user content property is a property that has been added to a
        /// published content type by Umbraco but does not corresponds to a user-defined
        /// published property.</remarks>
        public bool IsUserProperty { get; }

        /// <summary>
        /// Gets the content variations of the property type.
        /// </summary>
        public ContentVariation Variations { get; }

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

            foreach (var converter in _propertyValueConverters)
            {
                if (!converter.IsConverter(this))
                    continue;

                if (_converter == null)
                {
                    _converter = converter;
                    isdefault = _propertyValueConverters.IsDefault(converter);
                    continue;
                }

                if (isdefault)
                {
                    if (_propertyValueConverters.IsDefault(converter))
                    {
                        // previous was default, and got another default
                        if (_propertyValueConverters.Shadows(_converter, converter))
                        {
                            // previous shadows, ignore
                        }
                        else if (_propertyValueConverters.Shadows(converter, _converter))
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
                                ContentType.Alias, Alias,
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
                    if (_propertyValueConverters.IsDefault(converter))
                    {
                        // previous was non-default, ignore default
                    }
                    else
                    {
                        // previous was non-default, and got another non-default - bad
                        throw new InvalidOperationException(string.Format("Type '{2}' cannot be an IPropertyValueConverter"
                                                                          + " for property '{1}' of content type '{0}' because type '{3}' has already been detected as a converter"
                                                                          + " for that property, and only one converter can exist for a property.",
                            ContentType.Alias, Alias,
                            converter.GetType().FullName, _converter.GetType().FullName));
                    }
                }
            }

            _cacheLevel = _converter?.GetPropertyCacheLevel(this) ?? PropertyCacheLevel.Snapshot;
            _modelClrType = _converter == null ? typeof (object) : _converter.GetPropertyValueType(this);
        }

        /// <summary>
        /// Determines whether a value is an actual value, or not a value.
        /// </summary>
        /// <remarks>Used by property.HasValue and, for instance, in fallback scenarios.</remarks>
        public bool? IsValue(object value, PropertyValueLevel level)
        {
            if (!_initialized) Initialize();

            // if we have a converter, use the converter
            if (_converter != null)
                return _converter.IsValue(value, level);

            // otherwise use the old magic null & string comparisons
            return value != null && (!(value is string) || string.IsNullOrWhiteSpace((string) value) == false);
        }

        /// <summary>
        /// Gets the property cache level.
        /// </summary>
        public PropertyCacheLevel CacheLevel
        {
            get
            {
                if (!_initialized) Initialize();
                return _cacheLevel;
            }
        }

        /// <summary>
        /// Converts the source value into the intermediate value.
        /// </summary>
        /// <param name="owner">The published element owning the property.</param>
        /// <param name="source">The source value.</param>
        /// <param name="preview">A value indicating whether content should be considered draft.</param>
        /// <returns>The intermediate value.</returns>
        public object ConvertSourceToInter(IPublishedElement owner, object source, bool preview)
        {
            if (!_initialized) Initialize();

            // use the converter if any, else just return the source value
            return _converter != null
                ? _converter.ConvertSourceToIntermediate(owner, this, source, preview)
                : source;
        }

        /// <summary>
        /// Converts the intermediate value into the object value.
        /// </summary>
        /// <param name="owner">The published element owning the property.</param>
        /// <param name="referenceCacheLevel">The reference cache level.</param>
        /// <param name="inter">The intermediate value.</param>
        /// <param name="preview">A value indicating whether content should be considered draft.</param>
        /// <returns>The object value.</returns>
        public object ConvertInterToObject(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (!_initialized) Initialize();

            // use the converter if any, else just return the inter value
            return _converter != null
                ? _converter.ConvertIntermediateToObject(owner, this, referenceCacheLevel, inter, preview)
                : inter;
        }

        /// <summary>
        /// Converts the intermediate value into the XPath value.
        /// </summary>
        /// <param name="owner">The published element owning the property.</param>
        /// <param name="referenceCacheLevel">The reference cache level.</param>
        /// <param name="inter">The intermediate value.</param>
        /// <param name="preview">A value indicating whether content should be considered draft.</param>
        /// <returns>The XPath value.</returns>
        /// <remarks>
        /// <para>The XPath value can be either a string or an XPathNavigator.</para>
        /// </remarks>
        public object ConvertInterToXPath(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (!_initialized) Initialize();

            // use the converter if any
            if (_converter != null)
                return _converter.ConvertIntermediateToXPath(owner, this, referenceCacheLevel, inter, preview);

            // else just return the inter value as a string or an XPathNavigator
            if (inter == null) return null;
            if (inter is XElement xElement)
                return xElement.CreateNavigator();
            return inter.ToString().Trim();
        }

        /// <summary>
        /// Gets the property model common language runtime type.
        /// </summary>
        /// <remarks>
        /// <para>The model common language runtime type may be a <see cref="ModelType"/> type, or may contain <see cref="ModelType"/> types.</para>
        /// <para>For the actual common language runtime type, see <see cref="ClrType"/>.</para>
        /// </remarks>
        public Type ModelClrType
        {
            get
            {
                if (!_initialized) Initialize();
                return _modelClrType;
            }
        }

        /// <summary>
        /// Gets the property common language runtime type.
        /// </summary>
        /// <remarks>
        /// <para>Returns the actual common language runtime type which does not contain <see cref="ModelType"/> types.</para>
        /// <para>Mapping from <see cref="ModelClrType"/> may throw if some <see cref="ModelType"/> instances
        /// could not be mapped to actual common language runtime types.</para>
        /// </remarks>
        public Type ClrType
        {
            get
            {
                if (!_initialized) Initialize();
                return _clrType ?? (_clrType = _publishedModelFactory.MapModelType(_modelClrType));
            }
        }

        #endregion
    }
}
