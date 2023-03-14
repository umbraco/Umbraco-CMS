using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.Xml;

/// <summary>
///     The XmlHelper class contains general helper methods for working with xml in umbraco.
/// </summary>
public class XmlHelper
{
    /// <summary>
    ///     Creates or sets an attribute on the XmlNode if an Attributes collection is available
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="n"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void SetAttribute(XmlDocument xml, XmlNode n, string name, string value)
    {
        if (xml == null)
        {
            throw new ArgumentNullException(nameof(xml));
        }

        if (n == null)
        {
            throw new ArgumentNullException(nameof(n));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (n.Attributes == null)
        {
            return;
        }

        if (n.Attributes[name] == null)
        {
            XmlAttribute a = xml.CreateAttribute(name);
            a.Value = value;
            n.Attributes.Append(a);
        }
        else
        {
            n.Attributes[name]!.Value = value;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether a specified string contains only xml whitespace characters.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns><c>true</c> if the string contains only xml whitespace characters.</returns>
    /// <remarks>As per XML 1.1 specs, space, \t, \r and \n.</remarks>
    public static bool IsXmlWhitespace(string s)
    {
        // as per xml 1.1 specs - anything else is significant whitespace
        s = s.Trim(Constants.CharArrays.XmlWhitespaceChars);
        return s.Length == 0;
    }

    /// <summary>
    ///     Creates a new <c>XPathDocument</c> from an xml string.
    /// </summary>
    /// <param name="xml">The xml string.</param>
    /// <returns>An <c>XPathDocument</c> created from the xml string.</returns>
    public static XPathDocument CreateXPathDocument(string xml) =>
        new XPathDocument(new XmlTextReader(new StringReader(xml)));

    /// <summary>
    ///     Tries to create a new <c>XPathDocument</c> from an xml string.
    /// </summary>
    /// <param name="xml">The xml string.</param>
    /// <param name="doc">The XPath document.</param>
    /// <returns>A value indicating whether it has been possible to create the document.</returns>
    public static bool TryCreateXPathDocument(string xml, out XPathDocument? doc)
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
    ///     Tries to create a new <c>XPathDocument</c> from a property value.
    /// </summary>
    /// <param name="value">The value of the property.</param>
    /// <param name="doc">The XPath document.</param>
    /// <returns>A value indicating whether it has been possible to create the document.</returns>
    /// <remarks>The value can be anything... Performance-wise, this is bad.</remarks>
    public static bool TryCreateXPathDocumentFromPropertyValue(object value, out XPathDocument? doc)
    {
        // DynamicNode.ConvertPropertyValueByDataType first cleans the value by calling
        // XmlHelper.StripDashesInElementOrAttributeName - this is because the XML is
        // to be returned as a DynamicXml and element names such as "value-item" are
        // invalid and must be converted to "valueitem". But we don't have that sort of
        // problem here - and we don't need to bother with dashes nor dots, etc.
        doc = null;
        if (value is not string xml)
        {
            return false; // no a string
        }

        if (CouldItBeXml(xml) == false)
        {
            return false; // string does not look like it's xml
        }

        if (IsXmlWhitespace(xml))
        {
            return false; // string is whitespace, xml-wise
        }

        if (TryCreateXPathDocument(xml, out doc) == false)
        {
            return false; // string can't be parsed into xml
        }

        XPathNavigator nav = doc!.CreateNavigator();
        if (nav.MoveToFirstChild())
        {
            // SD: This used to do this but the razor macros and the entire razor macros section is gone, it was all legacy, it seems this method isn't even
            // used apart from for tests so don't think this matters. In any case, we no longer check for this!

            // var name = nav.LocalName; // must not match an excluded tag
            // if (UmbracoConfig.For.UmbracoSettings().Scripting.NotDynamicXmlDocumentElements.All(x => x.Element.InvariantEquals(name) == false)) return true;
            return true;
        }

        doc = null;
        return false;
    }

    /// <summary>
    ///     Sorts the children of a parentNode.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="childNodesXPath">An XPath expression to select children of <paramref name="parentNode" /> to sort.</param>
    /// <param name="orderBy">A function returning the value to order the nodes by.</param>
    public static void SortNodes(
        XmlNode parentNode,
        string childNodesXPath,
        Func<XmlNode, int> orderBy)
    {
        XmlNode[]? sortedChildNodes = parentNode.SelectNodes(childNodesXPath)?.Cast<XmlNode>()
            .OrderBy(orderBy)
            .ToArray();

        // append child nodes to last position, in sort-order
        // so all child nodes will go after the property nodes
        if (sortedChildNodes is not null)
        {
            foreach (XmlNode node in sortedChildNodes)
            {
                parentNode.AppendChild(node); // moves the node to the last position
            }
        }
    }

    /// <summary>
    ///     Sorts a single child node of a parentNode.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="childNodesXPath">An XPath expression to select children of <paramref name="parentNode" /> to sort.</param>
    /// <param name="node">The child node to sort.</param>
    /// <param name="orderBy">A function returning the value to order the nodes by.</param>
    /// <returns>A value indicating whether sorting was needed.</returns>
    /// <remarks>
    ///     Assuming all nodes but <paramref name="node" /> are sorted, this will move the node to
    ///     the right position without moving all the nodes (as SortNodes would do) - should improve perfs.
    /// </remarks>
    public static bool SortNode(
        XmlNode parentNode,
        string childNodesXPath,
        XmlNode node,
        Func<XmlNode, int> orderBy)
    {
        var nodeSortOrder = orderBy(node);
        Tuple<XmlNode, int>[]? childNodesAndOrder = parentNode.SelectNodes(childNodesXPath)?.Cast<XmlNode>()
            .Select(x => Tuple.Create(x, orderBy(x))).ToArray();

        // only one node = node is in the right place already, obviously
        if (childNodesAndOrder is null || childNodesAndOrder.Length == 1)
        {
            return false;
        }

        // find the first node with a sortOrder > node.sortOrder
        var i = 0;
        while (i < childNodesAndOrder.Length && childNodesAndOrder[i].Item2 <= nodeSortOrder)
        {
            i++;
        }

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
    ///     Opens a file as a XmlDocument.
    /// </summary>
    /// <param name="filePath">The relative file path. ie. /config/umbraco.config</param>
    /// <param name="hostingEnvironment"></param>
    /// <returns>Returns a XmlDocument class</returns>
    public static XmlDocument OpenAsXmlDocument(string filePath, IHostingEnvironment hostingEnvironment)
    {
        using (var reader =
               new XmlTextReader(hostingEnvironment.MapPathContentRoot(filePath))
               {
                   WhitespaceHandling = WhitespaceHandling.All,
               })
        {
            var xmlDoc = new XmlDocument();

            // Load the file into the XmlDocument
            xmlDoc.Load(reader);

            return xmlDoc;
        }
    }

    /// <summary>
    ///     creates a XmlAttribute with the specified name and value
    /// </summary>
    /// <param name="xd">The xmldocument.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="value">The value of the attribute.</param>
    /// <returns>a XmlAttribute</returns>
    public static XmlAttribute AddAttribute(XmlDocument xd, string name, string value)
    {
        if (xd == null)
        {
            throw new ArgumentNullException(nameof(xd));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        XmlAttribute temp = xd.CreateAttribute(name);
        temp.Value = value;
        return temp;
    }

    /// <summary>
    ///     Creates a text XmlNode with the specified name and value
    /// </summary>
    /// <param name="xd">The xmldocument.</param>
    /// <param name="name">The node name.</param>
    /// <param name="value">The node value.</param>
    /// <returns>a XmlNode</returns>
    public static XmlNode AddTextNode(XmlDocument xd, string name, string value)
    {
        if (xd == null)
        {
            throw new ArgumentNullException(nameof(xd));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        XmlNode temp = xd.CreateNode(XmlNodeType.Element, name, string.Empty);
        temp.AppendChild(xd.CreateTextNode(value));
        return temp;
    }

    /// <summary>
    ///     Sets or Creates a text XmlNode with the specified name and value
    /// </summary>
    /// <param name="xd">The xmldocument.</param>
    /// <param name="parent">The node to set or create the child text node on</param>
    /// <param name="name">The node name.</param>
    /// <param name="value">The node value.</param>
    /// <returns>a XmlNode</returns>
    public static XmlNode SetTextNode(XmlDocument xd, XmlNode parent, string name, string value)
    {
        if (xd == null)
        {
            throw new ArgumentNullException(nameof(xd));
        }

        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        XmlNode? child = parent.SelectSingleNode(name);
        if (child != null)
        {
            child.InnerText = value;
            return child;
        }

        return AddTextNode(xd, name, value);
    }

    /// <summary>
    ///     Sets or creates an Xml node from its inner Xml.
    /// </summary>
    /// <param name="xd">The xmldocument.</param>
    /// <param name="parent">The node to set or create the child text node on</param>
    /// <param name="name">The node name.</param>
    /// <param name="value">The node inner Xml.</param>
    /// <returns>a XmlNode</returns>
    public static XmlNode SetInnerXmlNode(XmlDocument xd, XmlNode parent, string name, string value)
    {
        if (xd == null)
        {
            throw new ArgumentNullException(nameof(xd));
        }

        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        XmlNode child = parent.SelectSingleNode(name) ?? xd.CreateNode(XmlNodeType.Element, name, string.Empty);
        child.InnerXml = value;
        return child;
    }

    /// <summary>
    ///     Creates a cdata XmlNode with the specified name and value
    /// </summary>
    /// <param name="xd">The xmldocument.</param>
    /// <param name="name">The node name.</param>
    /// <param name="value">The node value.</param>
    /// <returns>A XmlNode</returns>
    public static XmlNode AddCDataNode(XmlDocument xd, string name, string value)
    {
        if (xd == null)
        {
            throw new ArgumentNullException(nameof(xd));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        XmlNode temp = xd.CreateNode(XmlNodeType.Element, name, string.Empty);
        temp.AppendChild(xd.CreateCDataSection(value));
        return temp;
    }

    /// <summary>
    ///     Sets or Creates a cdata XmlNode with the specified name and value
    /// </summary>
    /// <param name="xd">The xmldocument.</param>
    /// <param name="parent">The node to set or create the child text node on</param>
    /// <param name="name">The node name.</param>
    /// <param name="value">The node value.</param>
    /// <returns>a XmlNode</returns>
    public static XmlNode SetCDataNode(XmlDocument xd, XmlNode parent, string name, string value)
    {
        if (xd == null)
        {
            throw new ArgumentNullException(nameof(xd));
        }

        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        XmlNode? child = parent.SelectSingleNode(name);
        if (child != null)
        {
            child.InnerXml = "<![CDATA[" + value + "]]>";
            return child;
        }

        return AddCDataNode(xd, name, value);
    }

    /// <summary>
    ///     Gets the value of a XmlNode
    /// </summary>
    /// <param name="n">The XmlNode.</param>
    /// <returns>the value as a string</returns>
    public static string GetNodeValue(XmlNode n)
    {
        var value = string.Empty;
        if (n == null || n.FirstChild == null)
        {
            return value;
        }

        value = n.FirstChild.Value ?? n.InnerXml;
        return value.Replace("<!--CDATAOPENTAG-->", "<![CDATA[").Replace("<!--CDATACLOSETAG-->", "]]>");
    }

    /// <summary>
    ///     Determines whether the specified string appears to be XML.
    /// </summary>
    /// <param name="xml">The XML string.</param>
    /// <returns>
    ///     <c>true</c> if the specified string appears to be XML; otherwise, <c>false</c>.
    /// </returns>
    public static bool CouldItBeXml(string? xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            return false;
        }

        xml = xml.Trim();
        return xml.StartsWith("<") && xml.EndsWith(">") && xml.Contains('/');
    }

    /// <summary>
    ///     Return a dictionary of attributes found for a string based tag
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetAttributesFromElement(string tag)
    {
        MatchCollection m =
            Regex.Matches(tag, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        // fix for issue 14862: return lowercase attributes for case insensitive matching
        var d = m.ToDictionary(
            attributeSet => attributeSet.Groups["attributeName"].Value.ToString().ToLower(),
            attributeSet => attributeSet.Groups["attributeValue"].Value.ToString());
        return d;
    }
}
