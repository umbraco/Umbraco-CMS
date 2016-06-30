using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Xml;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an <see cref="IPublishedProperty"/> type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedPropertyType"/> class are immutable, ie
    /// if the property type changes, then a new class needs to be created.</remarks>
    public class PublishedPropertyType
    {
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

            ContentType = contentType;
            PropertyTypeAlias = propertyType.Alias;

            DataTypeId = propertyType.DataTypeDefinitionId;
            PropertyEditorAlias = propertyType.PropertyEditorAlias;
        }

        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedPropertyType"/> class with an existing <see cref="PublishedPropertyType"/>
        /// and a new property type alias.
        /// </summary>
        /// <param name="propertyTypeAlias">The new property type alias.</param>
        /// <param name="propertyType">The existing published property type.</param>
        /// <remarks>
        /// <para>The new published property type does not belong to a published content type.</para>
        /// <para>It is a copy of the initial published property type, with a different alias.</para>
        /// </remarks>
        internal PublishedPropertyType(string propertyTypeAlias, PublishedPropertyType propertyType)
            : this(propertyTypeAlias, propertyType.DataTypeId, propertyType.PropertyEditorAlias)
        { }
        */

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
        internal PublishedPropertyType(string propertyTypeAlias, string propertyEditorAlias)
            : this(propertyTypeAlias, 0, propertyEditorAlias)
        { }

        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedPropertyType"/> class with a property type alias and a datatype definition.
        /// </summary>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="dataTypeDefinition">The datatype definition.</param>
        /// <remarks>
        /// <para>The new published property type does not belong to a published content type.</para>
        /// </remarks>
        internal PublishedPropertyType(string propertyTypeAlias, IDataTypeDefinition dataTypeDefinition)
            : this(propertyTypeAlias, dataTypeDefinition.Id, dataTypeDefinition.PropertyEditorAlias)
        { }
        */

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
        internal PublishedPropertyType(string propertyTypeAlias, int dataTypeDefinitionId, string propertyEditorAlias, bool umbraco = false)
        {
            // ContentType
            // - in unit tests, to be set by PublishedContentType when creating it

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
        internal bool IsUmbraco { get; private set; }

        #endregion

        #region Converters

        private readonly object _locker = new object();
        private volatile bool _initialized;
        private IPropertyValueConverter _converter;
        private PropertyCacheLevel _cacheLevel;

        private Type _clrType = typeof (object);

        private void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_locker)
            {
                if (_initialized) return;
                InitializeConverters();
                _initialized = true;
            }
        }

        private void InitializeConverters()
        {
            //TODO: Look at optimizing this method, it gets run for every property type for the document being rendered at startup,
            // every precious second counts!

            var converters = PropertyValueConvertersResolver.Current.Converters.ToArray();
            var defaultConvertersWithAttributes = PropertyValueConvertersResolver.Current.DefaultConverters;

            _converter = null;

            //get all converters for this property type
            var foundConverters = converters.Where(x => x.IsConverter(this)).ToArray();
            if (foundConverters.Length == 1)
            {
                _converter = foundConverters[0];
            }
            else if (foundConverters.Length > 1)
            {
                //more than one was found, we need to first figure out if one of these is an Umbraco default value type converter
                //get the non-default and see if we have one
                var nonDefault = foundConverters.Except(defaultConvertersWithAttributes.Select(x => x.Item1)).ToArray();
                if (nonDefault.Length == 1)
                {
                    //there's only 1 custom converter registered that so use it
                    _converter = nonDefault[0];
                }
                else if (nonDefault.Length > 1)
                {
                    //this is not allowed, there cannot be more than 1 custom converter
                    throw new InvalidOperationException(
                        string.Format("Type '{2}' cannot be an IPropertyValueConverter"
                                      + " for property '{1}' of content type '{0}' because type '{3}' has already been detected as a converter"
                                      + " for that property, and only one converter can exist for a property.",
                                      ContentType.Alias, PropertyTypeAlias,
                                      nonDefault[1].GetType().FullName, nonDefault[0].GetType().FullName));
                }
                else
                {
                    //we need to remove any converters that have been shadowed by another converter
                    var foundDefaultConvertersWithAttributes = defaultConvertersWithAttributes.Where(x => foundConverters.Contains(x.Item1));
                    var shadowedTypes = foundDefaultConvertersWithAttributes.SelectMany(x => x.Item2.DefaultConvertersToShadow);
                    var shadowedDefaultConverters = foundConverters.Where(x => shadowedTypes.Contains(x.GetType()));
                    var nonShadowedDefaultConverters = foundConverters.Except(shadowedDefaultConverters).ToArray();

                    if (nonShadowedDefaultConverters.Length == 1)
                    {
                        //assign to the single default converter
                        _converter = nonShadowedDefaultConverters[0];
                    }
                    else if (nonShadowedDefaultConverters.Length > 1)
                    {
                        //this is not allowed, there cannot be more than 1 custom converter
                        throw new InvalidOperationException(
                            string.Format("Type '{2}' cannot be an IPropertyValueConverter"
                                          + " for property '{1}' of content type '{0}' because type '{3}' has already been detected as a converter"
                                          + " for that property, and only one converter can exist for a property.",
                                          ContentType.Alias, PropertyTypeAlias,
                                          nonShadowedDefaultConverters[1].GetType().FullName, nonShadowedDefaultConverters[0].GetType().FullName));
                    }
                }

            }

            _cacheLevel = _converter?.GetPropertyCacheLevel(this) ?? PropertyCacheLevel.Facade;
            _clrType = _converter?.GetPropertyValueType(this) ?? typeof(object);
        }

        // gets the cache level
        public PropertyCacheLevel CacheLevel
        {
            get
            {
                EnsureInitialized();
                return _cacheLevel;
            }
        }

        // converts the source value into the inter value
        // uses converters, else falls back to dark (& performance-wise expensive) magic
        // source: the property source value
        // preview: whether we are previewing or not
        public object ConvertSourceToInter(object source, bool preview)
        {
            EnsureInitialized();

            // use the converter else use dark (& performance-wise expensive) magic
            return _converter != null
                ? _converter.ConvertSourceToInter(this, source, preview)
                : source;
        }

        // converts the inter value into the clr value
        // uses converters, else returns the inter value
        // inter: the property inter value
        // preview: whether we are previewing or not
        public object ConvertInterToObject(PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            EnsureInitialized();

            // use the converter if any
            // else just return the inter value
            return _converter != null
                ? _converter.ConvertInterToObject(this, referenceCacheLevel, inter, preview)
                : inter;
        }

        // converts the inter value into the xpath value
        // uses the converter else returns the inter value as a string
        // if successful, returns either a string or an XPathNavigator
        // inter: the property inter value
        // preview: whether we are previewing or not
        public object ConvertInterToXPath(PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            EnsureInitialized();

            // use the converter if any
            if (_converter != null)
                return _converter.ConvertInterToXPath(this, referenceCacheLevel, inter, preview);

            // else just return the inter value as a string or an XPathNavigator
            if (inter == null) return null;
            var xElement = inter as XElement;
            if (xElement != null)
                return xElement.CreateNavigator();
            return inter.ToString().Trim();
        }

        // gets the property CLR type
        public Type ClrType
        {
            get
            {
                EnsureInitialized();
                return _clrType;
            }
        }

        #endregion
    }
}
