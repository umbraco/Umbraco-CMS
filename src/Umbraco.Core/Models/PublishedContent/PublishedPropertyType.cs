using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Dynamics;
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
        #region Constructors

        // clone
        private PublishedPropertyType(PublishedPropertyType orig)
        {
            ContentType = orig.ContentType;
            PropertyTypeAlias = orig.PropertyTypeAlias;
            DataTypeId = orig.DataTypeId;
            PropertyEditorAlias = orig.PropertyEditorAlias;
            _converter = orig._converter;
            _sourceCacheLevel = orig._sourceCacheLevel;
            _objectCacheLevel = orig._objectCacheLevel;
            _xpathCacheLevel = orig._xpathCacheLevel;
            _clrType = orig._clrType;

            // do NOT copy the reduced cache levels
            // as we should NOT clone a nested / detached type
        }

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

            InitializeConverters();
        }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedPropertyType"/> class with a property type alias,
        /// a datatype definition identifier, and a property editor alias.
        /// </summary>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="dataTypeDefinitionId">The datatype definition identifier.</param>
        /// <param name="propertyEditorAlias">The property editor alias.</param>
        /// <remarks>
        /// <para>The new published property type does not belong to a published content type.</para>
        /// <para>The values of <paramref name="dataTypeDefinitionId"/> and <paramref name="propertyEditorAlias"/> are
        /// assumed to be valid and consistent.</para>
        /// </remarks>
        internal PublishedPropertyType(string propertyTypeAlias, int dataTypeDefinitionId, string propertyEditorAlias)
        {
            // ContentType 
            // - in unit tests, to be set by PublishedContentType when creating it
            // - in detached types, remains null

            PropertyTypeAlias = propertyTypeAlias;

            DataTypeId = dataTypeDefinitionId;
            PropertyEditorAlias = propertyEditorAlias;

            InitializeConverters();
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
        public string PropertyTypeAlias { get; private set; }

        /// <summary>
        /// Gets or sets the identifier uniquely identifying the data type supporting the property type.
        /// </summary>
        public int DataTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the alias uniquely identifying the property editor for the property type.
        /// </summary>
        public string PropertyEditorAlias { get; private set; }

        #endregion

        #region Converters

        private IPropertyValueConverter _converter;

        private PropertyCacheLevel _sourceCacheLevel;
        private PropertyCacheLevel _objectCacheLevel;
        private PropertyCacheLevel _xpathCacheLevel;

        private Type _clrType = typeof (object);

        private void InitializeConverters()
        {
            //TODO: Look at optimizing this method, it gets run for every property type for the document being rendered at startup,
            // every precious second counts!

            var converters = PropertyValueConvertersResolver.Current.Converters.ToArray();            
            var defaultConvertersWithAttributes = PropertyValueConvertersResolver.Current.DefaultConverters;

            _converter = null;
            
            //get all converters for this property type
            // todo: remove Union() once we drop IPropertyEditorValueConverter support.
            var foundConverters = converters.Union(GetCompatConverters()).Where(x => x.IsConverter(this)).ToArray();
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

            var converterMeta = _converter as IPropertyValueConverterMeta;

            // get the cache levels, quietely fixing the inconsistencies (no need to throw, really)
            if (converterMeta != null)
            {
                _sourceCacheLevel = converterMeta.GetPropertyCacheLevel(this, PropertyCacheValue.Source);
                _objectCacheLevel = converterMeta.GetPropertyCacheLevel(this, PropertyCacheValue.Object);
                _xpathCacheLevel = converterMeta.GetPropertyCacheLevel(this, PropertyCacheValue.XPath);
            }
            else
            {
                _sourceCacheLevel = GetCacheLevel(_converter, PropertyCacheValue.Source);
                _objectCacheLevel = GetCacheLevel(_converter, PropertyCacheValue.Object);
                _xpathCacheLevel = GetCacheLevel(_converter, PropertyCacheValue.XPath);
            }
            if (_objectCacheLevel < _sourceCacheLevel) _objectCacheLevel = _sourceCacheLevel;
            if (_xpathCacheLevel < _sourceCacheLevel) _xpathCacheLevel = _sourceCacheLevel;

            // get the CLR type of the converted value
            if (_converter != null)
            {
                if (converterMeta != null)
                {
                    _clrType = converterMeta.GetPropertyValueType(this);
                }
                else
                {
                    var attr = _converter.GetType().GetCustomAttribute<PropertyValueTypeAttribute>(false);
                    if (attr != null)
                        _clrType = attr.Type;
                }
            }
        }

        static PropertyCacheLevel GetCacheLevel(IPropertyValueConverter converter, PropertyCacheValue value)
        {
            if (converter == null)
                return PropertyCacheLevel.Request;

            var attr = converter.GetType().GetCustomAttributes<PropertyValueCacheAttribute>(false)
                .FirstOrDefault(x => x.Value == value || x.Value == PropertyCacheValue.All);

            return attr == null ? PropertyCacheLevel.Request : attr.Level;
        }
        
        // converts the raw value into the source value
        // uses converters, else falls back to dark (& performance-wise expensive) magic
        // source: the property raw value
        // preview: whether we are previewing or not
        public object ConvertDataToSource(object source, bool preview)
        {
            // use the converter else use dark (& performance-wise expensive) magic
            return _converter != null 
                ? _converter.ConvertDataToSource(this, source, preview) 
                : ConvertUsingDarkMagic(source);
        }

        // gets the source cache level
        public PropertyCacheLevel SourceCacheLevel { get { return _sourceCacheLevel; } }

        // converts the source value into the clr value
        // uses converters, else returns the source value
        // source: the property source value
        // preview: whether we are previewing or not
        public object ConvertSourceToObject(object source, bool preview)
        {
            // use the converter if any
            // else just return the source value
            return _converter != null
                ? _converter.ConvertSourceToObject(this, source, preview) 
                : source;
        }

        // gets the value cache level
        public PropertyCacheLevel ObjectCacheLevel { get { return _objectCacheLevel; } }

        // converts the source value into the xpath value
        // uses the converter else returns the source value as a string
        // if successful, returns either a string or an XPathNavigator
        // source: the property source value
        // preview: whether we are previewing or not
        public object ConvertSourceToXPath(object source, bool preview)
        {
            // use the converter if any
            if (_converter != null)
                return _converter.ConvertSourceToXPath(this, source, preview);

            // else just return the source value as a string or an XPathNavigator
            if (source == null) return null;
            var xElement = source as XElement;
            if (xElement != null)
                return xElement.CreateNavigator();
            return source.ToString().Trim();
        }

        // gets the xpath cache level
        public PropertyCacheLevel XPathCacheLevel { get { return _xpathCacheLevel; } }

        internal static object ConvertUsingDarkMagic(object source)
        {
            // convert to string
            var stringSource = source as string;
            if (stringSource == null) return source; // not a string => return the object
            stringSource = stringSource.Trim();
            if (stringSource.Length == 0) return null; // empty string => return null

            // try numbers and booleans
            // make sure we use the invariant culture ie a dot decimal point, comma is for csv
            // NOTE far from perfect: "01a" is returned as a string but "012" is returned as an integer...
            int i;
            if (int.TryParse(stringSource, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                return i;
            float f;
            if (float.TryParse(stringSource, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                return f;
            bool b;
            if (bool.TryParse(stringSource, out b))
                return b;

            //TODO: We can change this just like we do for the JSON converter - but to maintain compatibility might mean this still has to remain here

            // try xml - that is expensive, performance-wise
            XElement elt;
            if (XmlHelper.TryCreateXElementFromPropertyValue(stringSource, out elt))
                return new DynamicXml(elt); // xml => return DynamicXml for compatiblity's sake

            return source;
        }

        // gets the property CLR type
        public Type ClrType { get { return _clrType; } }

        #endregion

        #region Compat

        // backward-compatibility: support IPropertyEditorValueConverter while we have to
        // todo: remove once we drop IPropertyEditorValueConverter support.

        IEnumerable<IPropertyValueConverter> GetCompatConverters()
        {
            var propertyEditorGuid = LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias(PropertyEditorAlias, LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.ReturnNull);
            return PropertyEditorValueConvertersResolver.HasCurrent && propertyEditorGuid.HasValue
                ? PropertyEditorValueConvertersResolver.Current.Converters
                    .Where(x => x.IsConverterFor(propertyEditorGuid.Value, ContentType.Alias, PropertyTypeAlias))
                    .Select(x => new CompatConverter(x))
                : Enumerable.Empty<IPropertyValueConverter>();
        }

        private class CompatConverter : PropertyValueConverterBase
        {
            private readonly IPropertyEditorValueConverter _converter;

            public CompatConverter(IPropertyEditorValueConverter converter)
            {
                _converter = converter;
            }

            public override bool IsConverter(PublishedPropertyType propertyType)
            {
                return true;
            }

            public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
            {
                // NOTE: ignore preview, because IPropertyEditorValueConverter does not support it
                return _converter.ConvertPropertyValue(source).Result;
            }
        }

        #endregion

        #region Detached

        private PropertyCacheLevel _sourceCacheLevelReduced = 0;
        private PropertyCacheLevel _objectCacheLevelReduced = 0;
        private PropertyCacheLevel _xpathCacheLevelReduced = 0;

        internal bool IsDetachedOrNested
        {
            // enough to test source
            get { return _sourceCacheLevelReduced != 0; }
        }

        /// <summary>
        /// Creates a detached clone of this published property type.
        /// </summary>
        /// <returns>A detached clone of this published property type.</returns>
        /// <remarks>
        /// <para>Only a published property type that has not already been detached or nested, can be detached.</para>
        /// <para>Use detached published property type when creating detached properties outside of a published content.</para>
        /// </remarks>
        internal PublishedPropertyType Detached()
        {
            // verify
            if (IsDetachedOrNested)
                throw new Exception("PublishedPropertyType is already detached/nested.");

            var detached = new PublishedPropertyType(this);
            detached._sourceCacheLevel 
                = detached._objectCacheLevel 
                = detached._xpathCacheLevel 
                = PropertyCacheLevel.Content;
            // set to none to a) indicate it's detached / nested and b) make sure any nested
            // types switch all their cache to .Content
            detached._sourceCacheLevelReduced 
                = detached._objectCacheLevelReduced
                = detached._xpathCacheLevelReduced
                = PropertyCacheLevel.None;

            return detached;
        }

        /// <summary>
        /// Creates a nested clone of this published property type within a specified container published property type.
        /// </summary>
        /// <param name="containerType">The container published property type.</param>
        /// <returns>A nested clone of this published property type</returns>
        /// <remarks>
        /// <para>Only a published property type that has not already been detached or nested, can be nested.</para>
        /// <para>Use nested published property type when creating detached properties within a published content.</para>
        /// </remarks>
        internal PublishedPropertyType Nested(PublishedPropertyType containerType)
        {
            // verify
            if (IsDetachedOrNested)
                throw new Exception("PublishedPropertyType is already detached/nested.");

            var nested = new PublishedPropertyType(this);

            // before we reduce, both xpath and object are >= source, and
            // the way reduce works, the relative order of resulting xpath, object and source are preserved

            // Reduce() will set _xxxCacheLevelReduced thus indicating that the type is detached / nested

            Reduce(_sourceCacheLevel, _sourceCacheLevelReduced, ref nested._sourceCacheLevel, ref nested._sourceCacheLevelReduced);
            Reduce(_objectCacheLevel, _objectCacheLevelReduced, ref nested._objectCacheLevel, ref nested._objectCacheLevelReduced);
            Reduce(_xpathCacheLevel, _xpathCacheLevelReduced, ref nested._xpathCacheLevel, ref nested._xpathCacheLevelReduced);

            return nested;
        }

        private static void Reduce(
            PropertyCacheLevel containerCacheLevel, PropertyCacheLevel containerCacheLevelReduced,
            ref PropertyCacheLevel nestedCacheLevel, ref PropertyCacheLevel nestedCacheLevelReduced)
        {
            // initialize if required
            if (containerCacheLevelReduced == 0)
                containerCacheLevelReduced = containerCacheLevel;

            switch (containerCacheLevelReduced)
            {
                case PropertyCacheLevel.None:
                    // once .None, force .Content for everything
                    nestedCacheLevel = PropertyCacheLevel.Content;
                    nestedCacheLevelReduced = PropertyCacheLevel.None; // and propagate
                    break;

                case PropertyCacheLevel.Request:
                    // once .Request, force .Content for everything
                    nestedCacheLevel = PropertyCacheLevel.Content;
                    nestedCacheLevelReduced = PropertyCacheLevel.Request; // and propagate
                    break;

                case PropertyCacheLevel.Content:
                    // as long as .Content, accept anything
                    nestedCacheLevelReduced = nestedCacheLevel; // and it becomes the nested reduced
                    break;

                case PropertyCacheLevel.ContentCache:
                    // once .ContentCache, accept .Request and .Content but not .ContentCache
                    switch (nestedCacheLevel)
                    {
                        case PropertyCacheLevel.Request:
                        case PropertyCacheLevel.None:
                            // accept
                            nestedCacheLevelReduced = nestedCacheLevel; // and it becomes the nested reduced
                            break;
                        case PropertyCacheLevel.Content:
                            // accept
                            nestedCacheLevelReduced = PropertyCacheLevel.ContentCache; // and propagate
                            break;
                        case PropertyCacheLevel.ContentCache:
                            // force .Content
                            nestedCacheLevel = PropertyCacheLevel.Content;
                            nestedCacheLevelReduced = PropertyCacheLevel.ContentCache; // and propagate
                            break;
                        default:
                            throw new Exception("Unsupported PropertyCacheLevel value.");
                    }
                    break;

                default:
                    throw new Exception("Unsupported PropertyCacheLevel value.");
            }
        }

        #endregion
    }
}
