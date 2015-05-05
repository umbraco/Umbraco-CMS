using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <summary>
        /// Saves the xml document async
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
	    public static async Task SaveAsync(this XmlDocument xdoc, string filename)
	    {
            if (xdoc.DocumentElement == null)
                throw new XmlException("Cannot save xml document, there is no root element");

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (var xmlWriter = XmlWriter.Create(fs, new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = true
            }))
            {
                //NOTE: There are no nice methods to write it async, only flushing it async. We
                // could implement this ourselves but it'd be a very manual process.
                xdoc.WriteTo(xmlWriter);
                await xmlWriter.FlushAsync().ConfigureAwait(false);
            }
	    }

        public static bool HasAttribute(this XmlAttributeCollection attributes, string attributeName)
        {
            return attributes.Cast<XmlAttribute>().Any(x => x.Name == attributeName);
        }	
		
        /// <summary>
        /// Selects a list of XmlNode matching an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The list of XmlNode matching the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNodeList SelectNodes(this XmlNode source, string expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectNodes(source, expression, av);
        }

        /// <summary>
        /// Selects a list of XmlNode matching an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The list of XmlNode matching the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNodeList SelectNodes(this XmlNode source, XPathExpression expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectNodes(source, expression, av);
        }

        /// <summary>
        /// Selects a list of XmlNode matching an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The list of XmlNode matching the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNodeList SelectNodes(this XmlNode source, string expression, params XPathVariable[] variables)
        {
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return source.SelectNodes(expression);

            var iterator = source.CreateNavigator().Select(expression, variables);
            return XmlNodeListFactory.CreateNodeList(iterator);
        }

        /// <summary>
        /// Selects a list of XmlNode matching an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The list of XmlNode matching the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNodeList SelectNodes(this XmlNode source, XPathExpression expression, params XPathVariable[] variables)
        {
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return source.SelectNodes(expression);

            var iterator = source.CreateNavigator().Select(expression, variables);
            return XmlNodeListFactory.CreateNodeList(iterator);
        }

        /// <summary>
        /// Selects the first XmlNode that matches an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The first XmlNode that matches the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNode SelectSingleNode(this XmlNode source, string expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectSingleNode(source, expression, av);
        }

        /// <summary>
        /// Selects the first XmlNode that matches an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The first XmlNode that matches the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNode SelectSingleNode(this XmlNode source, XPathExpression expression, IEnumerable<XPathVariable> variables)
        {
            var av = variables == null ? null : variables.ToArray();
            return SelectSingleNode(source, expression, av);
        }

        /// <summary>
        /// Selects the first XmlNode that matches an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The first XmlNode that matches the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNode SelectSingleNode(this XmlNode source, string expression, params XPathVariable[] variables)
        {
            if (variables == null || variables.Length == 0 || variables[0] == null)
                return source.SelectSingleNode(expression);

            return SelectNodes(source, expression, variables).Cast<XmlNode>().FirstOrDefault();
        }

        /// <summary>
        /// Selects the first XmlNode that matches an XPath expression.
        /// </summary>
        /// <param name="source">A source XmlNode.</param>
        /// <param name="expression">An XPath expression.</param>
        /// <param name="variables">A set of XPathVariables.</param>
        /// <returns>The first XmlNode that matches the XPath expression.</returns>
        /// <remarks>
        /// <para>If <param name="variables" /> is <c>null</c>, or is empty, or contains only one single
        /// value which itself is <c>null</c>, then variables are ignored.</para>
        /// <para>The XPath expression should reference variables as <c>$var</c>.</para>
        /// </remarks>
        public static XmlNode SelectSingleNode(this XmlNode source, XPathExpression expression, params XPathVariable[] variables)
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

        ///// <summary>
        ///// Converts from an XElement to an XmlElement
        ///// </summary>
        ///// <param name="xElement"></param>
        ///// <returns></returns>
        //public static XmlNode ToXmlElement(this XContainer xElement)
        //{
        //    var xmlDocument = new XmlDocument();
        //    using (var xmlReader = xElement.CreateReader())
        //    {
        //        xmlDocument.Load(xmlReader);
        //    }
        //    return xmlDocument.DocumentElement;
        //}

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

        public static XmlNode GetXmlNode(this XContainer element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc.FirstChild;
            }
        }

        public static XmlNode GetXmlNode(this XContainer element, XmlDocument xmlDoc)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                xmlDoc.Load(xmlReader);
                return xmlDoc.DocumentElement;
            }
        }

	}
}