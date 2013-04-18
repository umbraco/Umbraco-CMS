using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Umbraco.Core
{

	/// <summary>
	/// Extension methods for xml objects
	/// </summary>
	internal static class XmlExtensions
	{

        public static bool HasAttribute(this XmlAttributeCollection attributes, string attributeName)
        {
            return attributes.Cast<XmlAttribute>().Any(x => x.Name == attributeName);
        }	
		
        /// <summary>
        /// Converts from an XDocument to an XmlDocument
        /// </summary>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        /// <summary>
        /// Converts from an XmlDocument to an XDocument
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        /// <summary>
        /// Converts from an XElement to an XmlElement
        /// </summary>
        /// <param name="xElement"></param>
        /// <returns></returns>
        public static XmlNode ToXmlElement(this XElement xElement)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xElement.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument.DocumentElement;
        }

        /// <summary>
        /// Converts from an XmlElement to an XElement
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <returns></returns>
        public static XElement ToXElement(this XmlNode xmlElement)
        {
            using (var nodeReader = new XmlNodeReader(xmlElement))
            {
                nodeReader.MoveToContent();
                return XElement.Load(nodeReader);
            }
        }

        public static T AttributeValue<T>(this XElement xml, string attributeName)
        {
            if (xml == null) throw new ArgumentNullException("xml");
            if (xml.HasAttributes == false) return default(T);

            if (xml.Attribute(attributeName) == null)
                return default(T);

            var val = xml.Attribute(attributeName).Value;
            var result = val.TryConvertTo<T>();
            if (result.Success)
                return result.Result;

            return default(T);
        }

		public static T AttributeValue<T>(this XmlNode xml, string attributeName)
		{
			if (xml == null) throw new ArgumentNullException("xml");
			if (xml.Attributes == null) return default(T);

			if (xml.Attributes[attributeName] == null)
				return default(T);

			var val = xml.Attributes[attributeName].Value;
			var result = val.TryConvertTo<T>();
			if (result.Success)
				return result.Result;

			return default(T);
		}

	}
}