using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public PublishedPropertyType(PublishedContentType contentType, PropertyType propertyType)
        {
            // PropertyEditor [1:n] DataTypeDefinition [1:n] PropertyType

            ContentType = contentType;
            PropertyTypeAlias = propertyType.Alias;

            DataTypeId = propertyType.DataTypeDefinitionId;
            PropertyEditorAlias = propertyType.PropertyEditorAlias;

            InitializeConverters();
        }

        // for unit tests
        internal PublishedPropertyType(string propertyTypeAlias, int dataTypeDefinitionId, string propertyEditorAlias)
        {
            // ContentType to be set by PublishedContentType when creating it
            PropertyTypeAlias = propertyTypeAlias;

            DataTypeId = dataTypeDefinitionId;
            PropertyEditorAlias = propertyEditorAlias;

            InitializeConverters();
        }

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
            
            // get the cache levels, quietely fixing the inconsistencies (no need to throw, really)
            _sourceCacheLevel = GetCacheLevel(_converter, PropertyCacheValue.Source);
            _objectCacheLevel = GetCacheLevel(_converter, PropertyCacheValue.Object);
            _objectCacheLevel = GetCacheLevel(_converter, PropertyCacheValue.XPath);
            if (_objectCacheLevel < _sourceCacheLevel) _objectCacheLevel = _sourceCacheLevel;
            if (_xpathCacheLevel < _sourceCacheLevel) _xpathCacheLevel = _sourceCacheLevel;

            if (_converter != null)
            {
                var attr = _converter.GetType().GetCustomAttribute<PropertyValueTypeAttribute>(false);
                if (attr != null)
                    _clrType = attr.Type;
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
    }
}
