using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core
{
    /// <summary>
	/// The XmlHelper class contains general helper methods for working with xml in umbraco.
    /// </summary>
    public class XmlHelper
    {
        /// <summary>
        /// Gets a value indicating whether a specified string contains only xml whitespace characters.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns><c>true</c> if the string contains only xml whitespace characters.</returns>
        /// <remarks>As per XML 1.1 specs, space, \t, \r and \n.</remarks>
        public static bool IsXmlWhitespace(string s)
        {
            // as per xml 1.1 specs - anything else is significant whitespace
            s = s.Trim(' ', '\t', '\r', '\n');
            return s.Length == 0;
        }

        /// <summary>
        /// Creates a new <c>XPathDocument</c> from an xml string.
        /// </summary>
        /// <param name="xml">The xml string.</param>
        /// <returns>An <c>XPathDocument</c> created from the xml string.</returns>
        public static XPathDocument CreateXPathDocument(string xml)
        {
            return new XPathDocument(new XmlTextReader(new StringReader(xml)));
        }

        /// <summary>
        /// Tries to create a new <c>XPathDocument</c> from an xml string.
        /// </summary>
        /// <param name="xml">The xml string.</param>
        /// <param name="doc">The XPath document.</param>
        /// <returns>A value indicating whether it has been possible to create the document.</returns>
        public static bool TryCreateXPathDocument(string xml, out XPathDocument doc)
        {
            try
            {
                doc = CreateXPathDocument(xml);
                return true;
            }
            catch (Exception)
            {
                doc = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to create a new <c>XPathDocument</c> from a property value.
        /// </summary>
        /// <param name="value">The value of the property.</param>
        /// <param name="doc">The XPath document.</param>
        /// <returns>A value indicating whether it has been possible to create the document.</returns>
        /// <remarks>The value can be anything... Performance-wise, this is bad.</remarks>
        public static bool TryCreateXPathDocumentFromPropertyValue(object value, out XPathDocument doc)
        {
            // DynamicNode.ConvertPropertyValueByDataType first cleans the value by calling
            // XmlHelper.StripDashesInElementOrAttributeName - this is because the XML is
            // to be returned as a DynamicXml and element names such as "value-item" are
            // invalid and must be converted to "valueitem". But we don't have that sort of
            // problem here - and we don't need to bother with dashes nor dots, etc.

            doc = null;
            var xml = value as string;
            if (xml == null) return false; // no a string
            if (CouldItBeXml(xml) == false) return false; // string does not look like it's xml
            if (IsXmlWhitespace(xml)) return false; // string is whitespace, xml-wise
            if (TryCreateXPathDocument(xml, out doc) == false) return false; // string can't be parsed into xml

            var nav = doc.CreateNavigator();
            if (nav.MoveToFirstChild())
            {
                var name = nav.LocalName; // must not match an excluded tag
                if (UmbracoConfig.For.UmbracoSettings().Scripting.NotDynamicXmlDocumentElements.All(x => x.Element.InvariantEquals(name) == false)) return true;
            }

            doc = null;
            return false;
        }

        /// <summary>
        /// Tries to create a new <c>XElement</c> from a property value.
        /// </summary>
        /// <param name="value">The value of the property.</param>
        /// <param name="elt">The Xml element.</param>
        /// <returns>A value indicating whether it has been possible to create the element.</returns>
        /// <remarks>The value can be anything... Performance-wise, this is bad.</remarks>
        public static bool TryCreateXElementFromPropertyValue(object value, out XElement elt)
        {
            // see note above in TryCreateXPathDocumentFromPropertyValue...

            elt = null;
            var xml = value as string;
            if (xml == null) return false; // not a string
            if (CouldItBeXml(xml) == false) return false; // string does not look like it's xml
            if (IsXmlWhitespace(xml)) return false; // string is whitespace, xml-wise

            try
            {
                elt = XElement.Parse(xml, LoadOptions.None);
            }
            catch
            {
                elt = null;
                return false; // string can't be parsed into xml
            }

            var name = elt.Name.LocalName; // must not match an excluded tag
            if (UmbracoConfig.For.UmbracoSettings().Scripting.NotDynamicXmlDocumentElements.All(x => x.Element.InvariantEquals(name) == false)) return true;

            elt = null;
            return false;
        }
        
        /// <summary>
        /// Sorts the children of a parentNode.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="childNodesXPath">An XPath expression to select children of <paramref name="parentNode"/> to sort.</param>
        /// <param name="orderBy">A function returning the value to order the nodes by.</param>
        internal static void SortNodes(
            XmlNode parentNode, 
            string childNodesXPath,
            Func<XmlNode, int> orderBy)
        {
            var sortedChildNodes = parentNode.SelectNodes(childNodesXPath).Cast<XmlNode>()
                .OrderBy(orderBy)
                .ToArray();

            // append child nodes to last position, in sort-order
            // so all child nodes will go after the property nodes
            foreach (var node in sortedChildNodes)
                parentNode.AppendChild(node); // moves the node to the last position
        }

        /// <summary>
        /// Sorts the children of a parentNode if needed.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="childNodesXPath">An XPath expression to select children of <paramref name="parentNode"/> to sort.</param>
        /// <param name="orderBy">A function returning the value to order the nodes by.</param>
        /// <returns>A value indicating whether sorting was needed.</returns>
        /// <remarks>same as SortNodes but will do nothing if nodes are already sorted - should improve performances.</remarks>
        internal static bool SortNodesIfNeeded(
            XmlNode parentNode,
            string childNodesXPath,
            Func<XmlNode, int> orderBy)
        {
            // ensure orderBy runs only once per node
            // checks whether nodes are already ordered
            // and actually sorts only if needed

            var childNodesAndOrder = parentNode.SelectNodes(childNodesXPath).Cast<XmlNode>()
                .Select(x => Tuple.Create(x, orderBy(x))).ToArray();

            var a = 0;
            foreach (var x in childNodesAndOrder)
            {
                if (a > x.Item2)
                {
                    a = -1;
                    break;
                }
                a = x.Item2;
            }

            if (a >= 0)
	            return false;

            // append child nodes to last position, in sort-order
            // so all child nodes will go after the property nodes
            foreach (var x in childNodesAndOrder.OrderBy(x => x.Item2))
                parentNode.AppendChild(x.Item1); // moves the node to the last position

            return true;
        }

        /// <summary>
        /// Sorts a single child node of a parentNode.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="childNodesXPath">An XPath expression to select children of <paramref name="parentNode"/> to sort.</param>
        /// <param name="node">The child node to sort.</param>
        /// <param name="orderBy">A function returning the value to order the nodes by.</param>
        /// <returns>A value indicating whether sorting was needed.</returns>
        /// <remarks>Assuming all nodes but <paramref name="node"/> are sorted, this will move the node to 
        /// the right position without moving all the nodes (as SortNodes would do) - should improve perfs.</remarks>
        internal static bool SortNode(
            XmlNode parentNode,
            string childNodesXPath,
            XmlNode node,
            Func<XmlNode, int> orderBy)
	    {
            var nodeSortOrder = orderBy(node);
            var childNodesAndOrder = parentNode.SelectNodes(childNodesXPath).Cast<XmlNode>()
                .Select(x => Tuple.Create(x, orderBy(x))).ToArray();

            // find the first node with a sortOrder > node.sortOrder
            var i = 0;
            while (i < childNodesAndOrder.Length && childNodesAndOrder[i].Item2 <= nodeSortOrder)
               i++;

            // if one was found
            if (i < childNodesAndOrder.Length)
            {
                // and node is just before, we're done already
                // else we need to move it right before the node that was found
                if (i > 0 && childNodesAndOrder[i - 1].Item1 != node)
                {
                    parentNode.InsertBefore(node, childNodesAndOrder[i].Item1);
                    return true;
                }
            }
            else
            {
                // and node is the last one, we're done already
                // else we need to append it as the last one
                if (i > 0 && childNodesAndOrder[i - 1].Item1 != node)
                {
                    parentNode.AppendChild(node);
                    return true;
                }
            }
            return false;
        }

        // used by DynamicNode only, see note in TryCreateXPathDocumentFromPropertyValue
        public static string StripDashesInElementOrAttributeNames(string xml)
        {
            using (var outputms = new MemoryStream())
            {
                using (TextWriter outputtw = new StreamWriter(outputms))
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var tw = new StreamWriter(ms))
                        {
                            tw.Write(xml);
                            tw.Flush();
                            ms.Position = 0;
                            using (var tr = new StreamReader(ms))
                            {
                                bool IsInsideElement = false, IsInsideQuotes = false;
                                int ic = 0;
                                while ((ic = tr.Read()) != -1)
                                {
                                    if (ic == (int)'<' && !IsInsideQuotes)
                                    {
                                        if (tr.Peek() != (int)'!')
                                        {
                                            IsInsideElement = true;
                                        }
                                    }
                                    if (ic == (int)'>' && !IsInsideQuotes)
                                    {
                                        IsInsideElement = false;
                                    }
                                    if (ic == (int)'"')
                                    {
                                        IsInsideQuotes = !IsInsideQuotes;
                                    }
                                    if (!IsInsideElement || ic != (int)'-' || IsInsideQuotes)
                                    {
                                        outputtw.Write((char)ic);
                                    }
                                }

                            }
                        }
                    }
                    outputtw.Flush();
                    outputms.Position = 0;
                    using (TextReader outputtr = new StreamReader(outputms))
                    {
                        return outputtr.ReadToEnd();
                    }
                }
            }
        }


		/// <summary>
        /// Imports a XML node from text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="xmlDoc">The XML doc.</param>
        /// <returns></returns>
		public static XmlNode ImportXmlNodeFromText(string text, ref XmlDocument xmlDoc)
        {
            xmlDoc.LoadXml(text);
            return xmlDoc.FirstChild;
        }

        /// <summary>
        /// Opens a file as a XmlDocument.
        /// </summary>
        /// <param name="filePath">The relative file path. ei. /config/umbraco.config</param>
        /// <returns>Returns a XmlDocument class</returns>
        public static XmlDocument OpenAsXmlDocument(string filePath)
        {

        	var reader = new XmlTextReader(IOHelper.MapPath(filePath)) {WhitespaceHandling = WhitespaceHandling.All};

        	var xmlDoc = new XmlDocument();
            //Load the file into the XmlDocument
            xmlDoc.Load(reader);
            //Close off the connection to the file.
            reader.Close();

            return xmlDoc;
        }

        /// <summary>
        /// creates a XmlAttribute with the specified name and value
        /// </summary>
        /// <param name="xd">The xmldocument.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <returns>a XmlAttribute</returns>
        public static XmlAttribute AddAttribute(XmlDocument xd, string name, string value)
        {
            var temp = xd.CreateAttribute(name);
            temp.Value = value;
            return temp;
        }

        /// <summary>
        /// Creates a text XmlNode with the specified name and value
        /// </summary>
        /// <param name="xd">The xmldocument.</param>
        /// <param name="name">The node name.</param>
        /// <param name="value">The node value.</param>
        /// <returns>a XmlNode</returns>
        public static XmlNode AddTextNode(XmlDocument xd, string name, string value)
        {
            var temp = xd.CreateNode(XmlNodeType.Element, name, "");
            temp.AppendChild(xd.CreateTextNode(value));
            return temp;
        }

        /// <summary>
        /// Creates a cdata XmlNode with the specified name and value
        /// </summary>
        /// <param name="xd">The xmldocument.</param>
        /// <param name="name">The node name.</param>
        /// <param name="value">The node value.</param>
        /// <returns>A XmlNode</returns>
		public static XmlNode AddCDataNode(XmlDocument xd, string name, string value)
        {
            var temp = xd.CreateNode(XmlNodeType.Element, name, "");
            temp.AppendChild(xd.CreateCDataSection(value));
            return temp;
        }

        /// <summary>
        /// Gets the value of a XmlNode
        /// </summary>
        /// <param name="n">The XmlNode.</param>
        /// <returns>the value as a string</returns>
		public static string GetNodeValue(XmlNode n)
        {
            var value = string.Empty;
            if (n == null || n.FirstChild == null)
                return value;
            value = n.FirstChild.Value ?? n.InnerXml;
            return value.Replace("<!--CDATAOPENTAG-->", "<![CDATA[").Replace("<!--CDATACLOSETAG-->", "]]>");
        }

        /// <summary>
        /// Determines whether the specified string appears to be XML.
        /// </summary>
        /// <param name="xml">The XML string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified string appears to be XML; otherwise, <c>false</c>.
        /// </returns>
		public static bool CouldItBeXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return false;

            xml = xml.Trim();
            return xml.StartsWith("<") && xml.EndsWith(">") && xml.Contains('/');
        }

        /// <summary>
        /// Splits the specified delimited string into an XML document.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="rootName">Name of the root.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>Returns an <c>System.Xml.XmlDocument</c> representation of the delimited string data.</returns>
		public static XmlDocument Split(string data, string[] separator, string rootName, string elementName)
        {
            return Split(new XmlDocument(), data, separator, rootName, elementName);
        }

        /// <summary>
        /// Splits the specified delimited string into an XML document.
        /// </summary>
        /// <param name="xml">The XML document.</param>
        /// <param name="data">The delimited string data.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="rootName">Name of the root node.</param>
        /// <param name="elementName">Name of the element node.</param>
        /// <returns>Returns an <c>System.Xml.XmlDocument</c> representation of the delimited string data.</returns>
		public static XmlDocument Split(XmlDocument xml, string data, string[] separator, string rootName, string elementName)
        {
            // load new XML document.
            xml.LoadXml(string.Concat("<", rootName, "/>"));

            // get the data-value, check it isn't empty.
            if (!string.IsNullOrEmpty(data))
            {
                // explode the values into an array
                var values = data.Split(separator, StringSplitOptions.None);

                // loop through the array items.
                foreach (string value in values)
                {
                    // add each value to the XML document.
                    var xn = XmlHelper.AddTextNode(xml, elementName, value);
                    xml.DocumentElement.AppendChild(xn);
                }
            }

            // return the XML node.
            return xml;
        }

		/// <summary>
		/// Return a dictionary of attributes found for a string based tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetAttributesFromElement(string tag)
		{
			var m =
				Regex.Matches(tag, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"",
							  RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			// fix for issue 14862: return lowercase attributes for case insensitive matching
			var d = m.Cast<Match>().ToDictionary(attributeSet => attributeSet.Groups["attributeName"].Value.ToString().ToLower(), attributeSet => attributeSet.Groups["attributeValue"].Value.ToString());
			return d;
		}
    }
}
