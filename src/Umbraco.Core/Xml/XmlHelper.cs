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
