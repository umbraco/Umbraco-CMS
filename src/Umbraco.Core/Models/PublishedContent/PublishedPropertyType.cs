using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents a published property type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedPropertyType"/> class are immutable, ie
    /// if the property type changes, then a new class needs to be created.</remarks>
    [DebuggerDisplay("{Alias} ({EditorAlias})")]
    public class PublishedPropertyType : IPublishedPropertyType
    {
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly PropertyValueConverterCollection _propertyValueConverters;
        private readonly object _locker = new object();
        private volatile bool _initialized;
        private IPropertyValueConverter? _converter;
        private PropertyCacheLevel _cacheLevel;

        private Type? _modelClrType;
        private Type? _clrType;

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="PublishedPropertyType"/> class with a property type.
        /// </summary>
        /// <remarks>
        /// <para>The new published property type belongs to the published content type.</para>
        /// </remarks>
        public PublishedPropertyType(IPublishedContentType contentType, IPropertyType propertyType, PropertyValueConverterCollection propertyValueConverters, IPublishedModelFactory publishedModelFactory, IPublishedContentTypeFactory factory)
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
        public PublishedPropertyType(IPublishedContentType contentType, string propertyTypeAlias, int dataTypeId, bool isUserProperty, ContentVariation variations, PropertyValueConverterCollection propertyValueConverters, IPublishedModelFactory publishedModelFactory, IPublishedContentTypeFactory factory)
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

        /// <inheritdoc />
        public IPublishedContentType? ContentType { get; internal set; } // internally set by PublishedContentType constructor

        /// <inheritdoc />
        public PublishedDataType DataType { get; }

        /// <inheritdoc />
        public string Alias { get; }

        /// <inheritdoc />
        public string EditorAlias => DataType.EditorAlias;

        /// <inheritdoc />
        public bool IsUserProperty { get; }

        /// <inheritdoc />
        public ContentVariation Variations { get; }

        #endregion

        #region Converters

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            lock (_locker)
            {
                if (_initialized)
                {
                    return;
                }

                InitializeLocked();
                _initialized = true;
            }
        }

        private void InitializeLocked()
        {
            _converter = null;
            var isdefault = false;

            foreach (IPropertyValueConverter converter in _propertyValueConverters)
            {
                if (!converter.IsConverter(this))
                {
                    continue;
                }

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
                            throw new InvalidOperationException(string.Format(
                                "Type '{2}' cannot be an IPropertyValueConverter"
                              + " for property '{1}' of content type '{0}' because type '{3}' has already been detected as a converter"
                              + " for that property, and only one converter can exist for a property.",
                                ContentType?.Alias,
                                Alias,
                                converter.GetType().FullName,
                                _converter.GetType().FullName));
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
                        throw new InvalidOperationException(string.Format(
                            "Type '{2}' cannot be an IPropertyValueConverter"
                          + " for property '{1}' of content type '{0}' because type '{3}' has already been detected as a converter"
                          + " for that property, and only one converter can exist for a property.",
                            ContentType?.Alias,
                            Alias,
                            converter.GetType().FullName,
                            _converter.GetType().FullName));
                    }
                }
            }

            _cacheLevel = _converter?.GetPropertyCacheLevel(this) ?? PropertyCacheLevel.Snapshot;
            _modelClrType = _converter == null ? typeof (object) : _converter.GetPropertyValueType(this);
        }

        /// <inheritdoc />
        public bool? IsValue(object? value, PropertyValueLevel level)
        {
            if (!_initialized)
            {
                Initialize();
            }

            // if we have a converter, use the converter
            if (_converter != null)
            {
                return _converter.IsValue(value, level);
            }

            // otherwise use the old magic null & string comparisons
            return value != null && (!(value is string) || string.IsNullOrWhiteSpace((string) value) == false);
        }

        /// <inheritdoc />
        public PropertyCacheLevel CacheLevel
        {
            get
            {
                if (!_initialized)
                {
                    Initialize();
                }

                return _cacheLevel;
            }
        }

        /// <inheritdoc />
        public object? ConvertSourceToInter(IPublishedElement owner, object? source, bool preview)
        {
            if (!_initialized)
            {
                Initialize();
            }

            // use the converter if any, else just return the source value
            return _converter != null
                ? _converter.ConvertSourceToIntermediate(owner, this, source, preview)
                : source;
        }

        /// <inheritdoc />
        public object? ConvertInterToObject(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            if (!_initialized)
            {
                Initialize();
            }

            // use the converter if any, else just return the inter value
            return _converter != null
                ? _converter.ConvertIntermediateToObject(owner, this, referenceCacheLevel, inter, preview)
                : inter;
        }

        /// <inheritdoc />
        public object? ConvertInterToXPath(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            if (!_initialized)
            {
                Initialize();
            }

            // use the converter if any
            if (_converter != null)
            {
                return _converter.ConvertIntermediateToXPath(owner, this, referenceCacheLevel, inter, preview);
            }

            // else just return the inter value as a string or an XPathNavigator
            if (inter == null)
            {
                return null;
            }

            if (inter is XElement xElement)
            {
                return xElement.CreateNavigator();
            }

            return inter.ToString()?.Trim();
        }

        /// <inheritdoc />
        public Type ModelClrType
        {
            get
            {
                if (!_initialized)
                {
                    Initialize();
                }

                return _modelClrType!;
            }
        }

        /// <inheritdoc />
        public Type? ClrType
        {
            get
            {
                if (!_initialized)
                {
                    Initialize();
                }

                return _clrType ?? (_modelClrType is not null ? _clrType = _publishedModelFactory.MapModelType(_modelClrType) : null);
            }
        }

        #endregion
    }
}
