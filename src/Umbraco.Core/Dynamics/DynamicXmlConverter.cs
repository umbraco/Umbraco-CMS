using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Umbraco.Core.Dynamics
{
    /// <summary>
    /// Used to return the raw xml string value from DynamicXml when using type converters
    /// </summary>
    public class RawXmlString
    {
        public string Value { get; private set; }

        public RawXmlString(string value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Used to return the raw xml XElement value from DynamicXml when using type converters
    /// </summary>
    public class RawXElement
    {
        public XElement Value { get; private set; }

        public RawXElement(XElement value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Used to return the raw xml XElement value from DynamicXml when using type converters
    /// </summary>
    public class RawXmlElement
    {
        public XmlElement Value { get; private set; }

        public RawXmlElement(XmlElement value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Used to return the raw xml XmlDocument value from DynamicXml when using type converters
    /// </summary>
    public class RawXmlDocument
    {
        public XmlDocument Value { get; private set; }

        public RawXmlDocument(XmlDocument value)
        {
            Value = value;
        }
    }

	/// <summary>
	/// A custom type converter for DynamicXml
	/// </summary>
	public class DynamicXmlConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			var convertableTypes = new[]
			    {
			        typeof(string), 
                    typeof(XElement), 
                    typeof(XmlElement), 
                    typeof(XmlDocument),
                    typeof(RawXmlString), 
                    typeof(RawXElement), 
                    typeof(RawXmlElement), 
                    typeof(RawXmlDocument)
			    };

			return convertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType)) 
			       || base.CanConvertFrom(context, destinationType);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context, 
			CultureInfo culture, 
			object value, 
			Type destinationType)
		{
			var dxml = value as DynamicXml;
			if (dxml == null)
				return null;
			//string
			if (TypeHelper.IsTypeAssignableFrom<string>(destinationType))
			{
				return value.ToString();
			}
            
			//XElement
			if (TypeHelper.IsTypeAssignableFrom<XElement>(destinationType))
			{
				return dxml.BaseElement;
			}
			//XmlElement
			if (TypeHelper.IsTypeAssignableFrom<XmlElement>(destinationType))
			{
				var xDoc = new XmlDocument();
				xDoc.LoadXml(dxml.ToString());
				return xDoc.DocumentElement;
			}
			//XmlDocument
			if (TypeHelper.IsTypeAssignableFrom<XmlDocument>(destinationType))
			{
				var xDoc = new XmlDocument();
				xDoc.LoadXml(dxml.ToString());
				return xDoc;
			}

            //RAW values:
            //string
            if (TypeHelper.IsTypeAssignableFrom<RawXmlString>(destinationType))
            {
                return new RawXmlString(dxml.ToRawXml());
            }
            //XElement
            if (TypeHelper.IsTypeAssignableFrom<RawXElement>(destinationType))
            {
                return new RawXElement(dxml.RawXmlElement);
            }
            //XmlElement
            if (TypeHelper.IsTypeAssignableFrom<RawXmlElement>(destinationType))
            {
                var xDoc = new XmlDocument();
                xDoc.LoadXml(dxml.ToRawXml());
                return new RawXmlElement(xDoc.DocumentElement);
            }
            //XmlDocument
            if (TypeHelper.IsTypeAssignableFrom<RawXmlDocument>(destinationType))
            {
                var xDoc = new XmlDocument();
                xDoc.LoadXml(dxml.ToRawXml());
                return new RawXmlDocument(xDoc);
            }


			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}