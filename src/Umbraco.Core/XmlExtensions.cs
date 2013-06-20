using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Xml;

namespace Umbraco.Core
{
	/// <summary>
	/// Extension methods for xml objects
	/// </summary>
	internal static class XmlExtensions
	{
        static XPathNodeIterator Select(string expression, XPathNavigator source, params XPathVariable[] variables)
        {
            var expr = source.Compile(expression);
            var context = new DynamicContext();
            foreach (var variable in variables)
                context.AddVariable(variable.Name, variable.Value);
            expr.SetContext(context);
            return source.Select(expr);
        }

        public static XmlNodeList SelectNodes(this XmlNode source, string expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectNodes(source, expression, av);
        }

        public static XmlNodeList SelectNodes(this XmlNode source, string expression, params XPathVariable[] variables)
        {
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return source.SelectNodes(expression);

            var iterator = Select(expression, source.CreateNavigator(), variables);
            return XmlNodeListFactory.CreateNodeList(iterator);
        }

        public static XmlNode SelectSingleNode(this XmlNode source, string expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectSingleNode(source, expression, av);
        }

        public static XmlNode SelectSingleNode(this XmlNode source, string expression, params XPathVariable[] variables)
        {
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return source.SelectSingleNode(expression);

            return SelectNodes(source, expression, variables).Cast<XmlNode>().FirstOrDefault();
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

        public static XElement GetXElement(this XmlNode node)
        {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        public static XmlNode GetXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc.FirstChild;
            }
        }

        public static XmlNode GetXmlNode(this XElement element, XmlDocument xmlDoc)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                xmlDoc.Load(xmlReader);
                return xmlDoc.DocumentElement;
            }
        }
	}
}