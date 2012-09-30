using System;
using System.Xml;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Web.Templates;

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

		private string _parsedValue;

		/// <summary>
		/// Returns the value of a property from the XML cache
		/// </summary>
		/// <remarks>
		/// This ensures that the result has any {localLink} syntax parsed and that urls are resolved correctly.
		/// This also ensures that the parsing is only done once as the result is cached in a private field of this object.
		/// </remarks>
		public object Value
		{
			get
			{
				if (_parsedValue == null)
				{
					_parsedValue = TemplateUtilities.ResolveUrlsFromTextString(
						TemplateUtilities.ParseInternalLinks(
							_value));	
				}
				return _parsedValue;
			}
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