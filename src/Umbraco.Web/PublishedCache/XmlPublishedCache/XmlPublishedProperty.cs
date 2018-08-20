using System;
using System.Xml;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{

	/// <summary>
	/// Represents an IDocumentProperty which is created based on an Xml structure.
	/// </summary>
	[Serializable]
	[XmlType(Namespace = "http://umbraco.org/webservices/")]
	internal class XmlPublishedProperty : PublishedPropertyBase
	{
		private readonly string _xmlValue; // the raw, xml node value

        // in v7 we're not using XPath value so don't allocate that Lazy.
        // as for the rest... we're single threaded here, keep it simple
        //private readonly Lazy<object> _sourceValue;
        //private readonly Lazy<object> _objectValue;
        //private readonly Lazy<object> _xpathValue;
        private object _objectValue;
        private bool _objectValueComputed;
        private readonly bool _isPreviewing;

        /// <summary>
        /// Gets the raw value of the property.
        /// </summary>
        public override object DataValue { get { return _xmlValue; } }

        // in the Xml cache, everything is a string, and to have a value
        // you want to have a non-null, non-empty string.
	    public override bool HasValue 
        {
	        get { return _xmlValue.Trim().Length > 0; }
	    }

	    public override object Value
	    {
	        get
	        {
                // NOT caching the source (intermediate) value since we'll never need it
                // everything in Xml cache in v7 is per-request anyways
                // also, properties should not be shared between requests and therefore
                // are single threaded, so the following code should be safe & fast

                if (_objectValueComputed) return _objectValue;
                var sourceValue = PropertyType.ConvertDataToSource(_xmlValue, _isPreviewing);
                _objectValue = PropertyType.ConvertSourceToObject(sourceValue, _isPreviewing);
                _objectValueComputed = true;
                return _objectValue;
            }
        }

        public override object XPathValue { get { throw new NotImplementedException(); } }

        public XmlPublishedProperty(PublishedPropertyType propertyType, bool isPreviewing, XmlNode propertyXmlData)
            : this(propertyType, isPreviewing)
		{
		    if (propertyXmlData == null)
		        throw new ArgumentNullException("propertyXmlData", "Property xml source is null");
		    _xmlValue = XmlHelper.GetNodeValue(propertyXmlData);
        }

        public XmlPublishedProperty(PublishedPropertyType propertyType, bool isPreviewing, string propertyData)
            : this(propertyType, isPreviewing)
        {
            if (propertyData == null)
                throw new ArgumentNullException("propertyData");
            _xmlValue = propertyData;
        }

        public XmlPublishedProperty(PublishedPropertyType propertyType, bool isPreviewing)
            : base(propertyType)
        {
            _xmlValue = string.Empty;
            _isPreviewing = isPreviewing;

            //_sourceValue = new Lazy<object>(() => PropertyType.ConvertDataToSource(_xmlValue, _isPreviewing));
            //_objectValue = new Lazy<object>(() => PropertyType.ConvertSourceToObject(_sourceValue.Value, _isPreviewing));
            //_xpathValue = new Lazy<object>(() => PropertyType.ConvertSourceToXPath(_sourceValue.Value, _isPreviewing));
        }
	}
}