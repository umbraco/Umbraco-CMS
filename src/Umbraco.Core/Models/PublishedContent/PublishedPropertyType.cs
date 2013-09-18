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
        public PublishedPropertyType(PublishedContentType contentType, PropertyType propertyType)
        {
            // one control identified by its DataTypeGuid
            // can be used to create several datatypes, identified by their DataTypeDefinitionId and supporting prevalues
            // which can be used to create several property types, identified by their Id

            ContentType = contentType;
            Id = propertyType.Id;
            Alias = propertyType.Alias;

            DataTypeId = propertyType.DataTypeDefinitionId;
            PropertyEditorGuid = propertyType.DataTypeId;

            InitializeConverters();
        }

        // for unit tests
        internal PublishedPropertyType(string alias, Guid propertyEditorGuid, int propertyTypeId, int dataTypeDefinitionId)
        {
            // ContentType to be set by PublishedContentType when creating it
            Id = propertyTypeId;
            Alias = alias;

            DataTypeId = dataTypeDefinitionId;
            PropertyEditorGuid = propertyEditorGuid;

            InitializeConverters();
        }

        #region Property type

        // gets the content type
        // internally set by PublishedContentType constructor
        public PublishedContentType ContentType { get; internal set; }

        // gets the property type id
        public int Id { get; private set; }

        // gets the property alias
        public string Alias { get; private set; }

        public int DataTypeId { get; private set; }

        public Guid PropertyEditorGuid { get; private set; }

        #endregion

        #region Converters

        private IPropertyValueConverter _sourceConverter;
        private IPropertyValueConverter _objectConverter;
        private IPropertyValueConverter _xpathConverter;

        private PropertyCacheLevel _sourceCacheLevel;
        private PropertyCacheLevel _objectCacheLevel;
        private PropertyCacheLevel _xpathCacheLevel;

        private void InitializeConverters()
        {
            var converters = PropertyValueConvertersResolver.Current.Converters.ToArray();

            // fixme - get rid of the IPropertyValueEditorConverter support eventually
            _sourceConverter = GetSingleConverterOrDefault(converters.Union(GetCompatConverters()), x => x.IsDataToSourceConverter(this), "data-to-source");
            _sourceCacheLevel = GetCacheLevel(_sourceConverter, PropertyCacheValue.Source);

            _objectConverter = GetSingleConverterOrDefault(converters, x => x.IsSourceToObjectConverter(this), "source-to-object");
            _objectCacheLevel = GetCacheLevel(_objectConverter, PropertyCacheValue.Object);
            if (_objectCacheLevel < _sourceCacheLevel)
                _objectCacheLevel = _sourceCacheLevel; // quietely fix the inconsistency, no need to throw

            _xpathConverter = GetSingleConverterOrDefault(converters, x => x.IsSourceToXPathConverter(this), "source-to-xpath");
            _objectCacheLevel = GetCacheLevel(_objectConverter, PropertyCacheValue.XPath);
            if (_xpathCacheLevel < _sourceCacheLevel)
                _xpathCacheLevel = _sourceCacheLevel; // quietely fix the inconsistency, no need to throw
        }

        static IPropertyValueConverter GetSingleConverterOrDefault(IEnumerable<IPropertyValueConverter> converters,
            Func<IPropertyValueConverter, bool> predicate, string name)
        {
            IPropertyValueConverter result = null;
            foreach (var converter in converters.Where(predicate))
            {
                if (result == null) result = converter;
                else throw new InvalidOperationException("More than one " + name + " converter.");
            }
            return result;
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
            return _sourceConverter != null 
                ? _sourceConverter.ConvertDataToSource(this, source, preview) 
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
            return _objectConverter != null 
                ? _objectConverter.ConvertSourceToObject(this, source, preview) 
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
            if (_xpathConverter != null)
                return _xpathConverter.ConvertSourceToXPath(this, source, preview);

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

            // try xml - that is expensive, performance-wise
            XElement elt;
            if (XmlHelper.TryCreateXElementFromPropertyValue(stringSource, out elt))
                return new DynamicXml(elt); // xml => return DynamicXml for compatiblity's sake

            return source;
        }

        #endregion

        #region Compat

        // fixme - remove in v7
        // backward-compatibility: support IPropertyEditorValueConverter while we have to
        IEnumerable<IPropertyValueConverter> GetCompatConverters()
        {
            return PropertyEditorValueConvertersResolver.HasCurrent
                ? PropertyEditorValueConvertersResolver.Current.Converters
                    .Where(x => x.IsConverterFor(PropertyEditorGuid, ContentType.Alias, Alias))
                    .Select(x => new CompatConverter(x))
                : Enumerable.Empty<IPropertyValueConverter>();
        }

        class CompatConverter : PropertyValueConverterBase
        {
            private readonly IPropertyEditorValueConverter _converter;

            public CompatConverter(IPropertyEditorValueConverter converter)
            {
                _converter = converter;
            }

            public override bool IsDataToSourceConverter(PublishedPropertyType propertyType)
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
