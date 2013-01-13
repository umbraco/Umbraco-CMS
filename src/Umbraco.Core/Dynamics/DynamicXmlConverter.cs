using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Umbraco.Core.Dynamics
{
	/// <summary>
	/// A custom type converter for DynamicXml
	/// </summary>
	public class DynamicXmlConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type sourceType)
		{
			var convertableTypes = new[] {typeof(string), typeof(XElement), typeof(XmlElement), typeof(XmlDocument)};

			return convertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, sourceType)) 
			       || base.CanConvertFrom(context, sourceType);
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
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}