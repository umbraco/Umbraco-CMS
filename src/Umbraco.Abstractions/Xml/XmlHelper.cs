using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Xml
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
                //SD: This used to do this but the razor macros and the entire razor macros section is gone, it was all legacy, it seems this method isn't even
                // used apart from for tests so don't think this matters. In any case, we no longer check for this!

                //var name = nav.LocalName; // must not match an excluded tag
                //if (UmbracoConfig.For.UmbracoSettings().Scripting.NotDynamicXmlDocumentElements.All(x => x.Element.InvariantEquals(name) == false)) return true;

                return true;
            }

            doc = null;
            return false;
        }


        /// <summary>
        /// Sorts the children of a parentNode.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="childNodesXPath">An XPath expression to select children of <paramref name="parentNode"/> to sort.</param>
        /// <param name="orderBy">A function returning the value to order the nodes by.</param>
        public static void SortNodes(
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
        /// Sorts a single child node of a parentNode.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="childNodesXPath">An XPath expression to select children of <paramref name="parentNode"/> to sort.</param>
        /// <param name="node">The child node to sort.</param>
        /// <param name="orderBy">A function returning the value to order the nodes by.</param>
        /// <returns>A value indicating whether sorting was needed.</returns>
        /// <remarks>Assuming all nodes but <paramref name="node"/> are sorted, this will move the node to
        /// the right position without moving all the nodes (as SortNodes would do) - should improve perfs.</remarks>
        public static bool SortNode(
            XmlNode parentNode,
            string childNodesXPath,
            XmlNode node,
            Func<XmlNode, int> orderBy)
        {
            var nodeSortOrder = orderBy(node);
            var childNodesAndOrder = parentNode.SelectNodes(childNodesXPath).Cast<XmlNode>()
                .Select(x => Tuple.Create(x, orderBy(x))).ToArray();

            // only one node = node is in the right place already, obviously
            if (childNodesAndOrder.Length == 1) return false;

            // find the first node with a sortOrder > node.sortOrder
            var i = 0;
            while (i < childNodesAndOrder.Length && childNodesAndOrder[i].Item2 <= nodeSortOrder)
               i++;

            // if one was found
            if (i < childNodesAndOrder.Length)
            {
                // and node is just before, we're done already
                // else we need to move it right before the node that was found
                if (i == 0 || childNodesAndOrder[i - 1].Item1 != node)
                {
                    parentNode.InsertBefore(node, childNodesAndOrder[i].Item1);
                    return true;
                }
            }
            else // i == childNodesAndOrder.Length && childNodesAndOrder.Length > 1
            {
                // and node is the last one, we're done already
                // else we need to append it as the last one
                // (and i > 1, see above)
                if (childNodesAndOrder[i - 1].Item1 != node)
                {
                    parentNode.AppendChild(node);
                    return true;
                }
            }
            return false;
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
        ///     <c>true</c> if the specified string appears to be XML; otherwise, <c>false</c>.
        /// </returns>
        public static bool CouldItBeXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return false;

            xml = xml.Trim();
            return xml.StartsWith("<") && xml.EndsWith(">") && xml.Contains('/');
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
