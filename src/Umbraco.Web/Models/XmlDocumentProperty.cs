using System;
using System.Xml;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{

	/// <summary>
	/// Represents an IDocumentProperty which is created based on an Xml structure.
	/// </summary>
	[Serializable]
	[XmlType(Namespace = "http://umbraco.org/webservices/")]
	public class XmlDocumentProperty : IDocumentProperty
	{
		private readonly Guid _version;
		private readonly string _alias;
		private readonly string _value;

		public string Alias
		{
			get { return _alias; }
		}

		public object Value
		{
			get { return IOHelper.ResolveUrlsFromTextString(_value); }
		}

		public Guid Version
		{
			get { return _version; }
		}

		public XmlDocumentProperty()
		{

		}

		public XmlDocumentProperty(XmlNode propertyXmlData)
		{
			if (propertyXmlData != null)
			{
				// For backward compatibility with 2.x (the version attribute has been removed from 3.0 data nodes)
				if (propertyXmlData.Attributes.GetNamedItem("versionID") != null)
					_version = new Guid(propertyXmlData.Attributes.GetNamedItem("versionID").Value);
				_alias = UmbracoSettings.UseLegacyXmlSchema ?
				                                            	propertyXmlData.Attributes.GetNamedItem("alias").Value :
				                                            	                                                       	propertyXmlData.Name;
				_value = XmlHelper.GetNodeValue(propertyXmlData);
			}
			else
				throw new ArgumentNullException("Property xml source is null");
		}
	}
}