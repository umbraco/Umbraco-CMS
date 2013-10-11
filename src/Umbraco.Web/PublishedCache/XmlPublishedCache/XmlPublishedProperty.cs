using System;
using System.Xml;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

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
	    private readonly Lazy<object> _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;
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

        public override object Value { get { return _objectValue.Value; } }
        public override object XPathValue { get { return _xpathValue.Value; } }

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

            _sourceValue = new Lazy<object>(() => PropertyType.ConvertDataToSource(_xmlValue, _isPreviewing));
            _objectValue = new Lazy<object>(() => PropertyType.ConvertSourceToObject(_sourceValue.Value, _isPreviewing));
            _xpathValue = new Lazy<object>(() => PropertyType.ConvertSourceToXPath(_sourceValue.Value, _isPreviewing));
        }
	}
}