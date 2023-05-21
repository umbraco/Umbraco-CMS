// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Xml;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for xml objects
/// </summary>
public static class XmlExtensions
{
    public static bool HasAttribute(this XmlAttributeCollection attributes, string attributeName) =>
        attributes.Cast<XmlAttribute>().Any(x => x.Name == attributeName);

    /// <summary>
    ///     Selects a list of XmlNode matching an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The list of XmlNode matching the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNodeList? SelectNodes(this XmlNode source, string expression, IEnumerable<XPathVariable>? variables)
    {
        XPathVariable[]? av = variables?.ToArray();
        return SelectNodes(source, expression, av);
    }

    /// <summary>
    ///     Selects a list of XmlNode matching an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The list of XmlNode matching the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNodeList? SelectNodes(this XmlNode source, XPathExpression expression, IEnumerable<XPathVariable>? variables)
    {
        XPathVariable[]? av = variables?.ToArray();
        return SelectNodes(source, expression, av);
    }

    /// <summary>
    ///     Selects a list of XmlNode matching an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The list of XmlNode matching the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNodeList? SelectNodes(this XmlNode source, string? expression, params XPathVariable[]? variables)
    {
        if (variables == null || variables.Length == 0 || variables[0] == null)
        {
            return source.SelectNodes(expression ?? string.Empty);
        }

        XPathNodeIterator? iterator = source.CreateNavigator()?.Select(expression ?? string.Empty, variables);
        return XmlNodeListFactory.CreateNodeList(iterator);
    }

    /// <summary>
    ///     Selects a list of XmlNode matching an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The list of XmlNode matching the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNodeList SelectNodes(this XmlNode source, XPathExpression expression, params XPathVariable[]? variables)
    {
        if (variables == null || variables.Length == 0 || variables[0] == null)
        {
            return source.SelectNodes(expression);
        }

        XPathNodeIterator? iterator = source.CreateNavigator()?.Select(expression, variables);
        return XmlNodeListFactory.CreateNodeList(iterator);
    }

    /// <summary>
    ///     Selects the first XmlNode that matches an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The first XmlNode that matches the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNode? SelectSingleNode(this XmlNode source, string expression, IEnumerable<XPathVariable>? variables)
    {
        XPathVariable[]? av = variables?.ToArray();
        return SelectSingleNode(source, expression, av);
    }

    /// <summary>
    ///     Selects the first XmlNode that matches an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The first XmlNode that matches the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNode? SelectSingleNode(this XmlNode source, XPathExpression expression, IEnumerable<XPathVariable>? variables)
    {
        XPathVariable[]? av = variables?.ToArray();
        return SelectSingleNode(source, expression, av);
    }

    /// <summary>
    ///     Selects the first XmlNode that matches an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The first XmlNode that matches the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNode? SelectSingleNode(this XmlNode source, string expression, params XPathVariable[]? variables)
    {
        if (variables == null || variables.Length == 0 || variables[0] == null)
        {
            return source.SelectSingleNode(expression);
        }

        return SelectNodes(source, expression, variables)?.Cast<XmlNode>().FirstOrDefault();
    }

    /// <summary>
    ///     Selects the first XmlNode that matches an XPath expression.
    /// </summary>
    /// <param name="source">A source XmlNode.</param>
    /// <param name="expression">An XPath expression.</param>
    /// <param name="variables">A set of XPathVariables.</param>
    /// <returns>The first XmlNode that matches the XPath expression.</returns>
    /// <remarks>
    ///     <para>
    ///         If
    ///         <param name="variables" />
    ///         is <c>null</c>, or is empty, or contains only one single
    ///         value which itself is <c>null</c>, then variables are ignored.
    ///     </para>
    ///     <para>The XPath expression should reference variables as <c>$var</c>.</para>
    /// </remarks>
    public static XmlNode? SelectSingleNode(this XmlNode source, XPathExpression expression, params XPathVariable[]? variables)
    {
        if (variables == null || variables.Length == 0 || variables[0] == null)
        {
            return source.SelectSingleNode(expression);
        }

        return SelectNodes(source, expression, variables).Cast<XmlNode>().FirstOrDefault();
    }

    /// <summary>
    ///     Converts from an XDocument to an XmlDocument
    /// </summary>
    /// <param name="xDocument"></param>
    /// <returns></returns>
    public static XmlDocument ToXmlDocument(this XDocument xDocument)
    {
        var xmlDocument = new XmlDocument();
        using (XmlReader xmlReader = xDocument.CreateReader())
        {
            xmlDocument.Load(xmlReader);
        }

        return xmlDocument;
    }

    /// <summary>
    ///     Converts from an XmlDocument to an XDocument
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
    public static XmlNode? ToXmlElement(this XContainer xElement)
    {
        var xmlDocument = new XmlDocument();
        using (XmlReader xmlReader = xElement.CreateReader())
        {
            xmlDocument.Load(xmlReader);
        }

        return xmlDocument.DocumentElement;
    }

    /// <summary>
    ///     Converts from an XmlElement to an XElement
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

    public static T? RequiredAttributeValue<T>(this XElement xml, string attributeName)
    {
        if (xml == null)
        {
            throw new ArgumentNullException(nameof(xml));
        }

        if (xml.HasAttributes == false)
        {
            throw new InvalidOperationException($"{attributeName} not found in xml");
        }

        XAttribute? attribute = xml.Attribute(attributeName);
        if (attribute is null)
        {
            throw new InvalidOperationException($"{attributeName} not found in xml");
        }

        Attempt<T> result = attribute.Value.TryConvertTo<T>();
        if (result.Success)
        {
            return result.Result;
        }

        throw new InvalidOperationException($"{attribute.Value} attribute value cannot be converted to {typeof(T)}");
    }

    public static T? AttributeValue<T>(this XElement xml, string attributeName)
    {
        if (xml == null)
        {
            throw new ArgumentNullException("xml");
        }

        if (xml.HasAttributes == false)
        {
            return default;
        }

        if (xml.Attribute(attributeName) == null)
        {
            return default;
        }

        var val = xml.Attribute(attributeName)?.Value;
        Attempt<T> result = val.TryConvertTo<T>();
        if (result.Success)
        {
            return result.Result;
        }

        return default;
    }

    public static T? AttributeValue<T>(this XmlNode xml, string attributeName)
    {
        if (xml == null)
        {
            throw new ArgumentNullException("xml");
        }

        if (xml.Attributes == null)
        {
            return default;
        }

        if (xml.Attributes[attributeName] == null)
        {
            return default;
        }

        var val = xml.Attributes[attributeName]?.Value;
        Attempt<T> result = val.TryConvertTo<T>();
        if (result.Success)
        {
            return result.Result;
        }

        return default;
    }

    public static XElement? GetXElement(this XmlNode node)
    {
        var xDoc = new XDocument();
        using (XmlWriter xmlWriter = xDoc.CreateWriter())
        {
            node.WriteTo(xmlWriter);
        }

        return xDoc.Root;
    }

    public static XmlNode? GetXmlNode(this XContainer element)
    {
        using (XmlReader xmlReader = element.CreateReader())
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlReader);
            return xmlDoc.DocumentElement;
        }
    }

    public static XmlNode? GetXmlNode(this XContainer element, XmlDocument xmlDoc)
    {
        XmlNode? node = element.GetXmlNode();
        if (node is not null)
        {
            return xmlDoc.ImportNode(node, true);
        }

        return null;
    }

    // this exists because
    // new XElement("root", "a\nb").Value is "a\nb" but
    // .ToString(SaveOptions.*) is "a\r\nb" and cannot figure out how to get rid of "\r"
    // and when saving data we want nothing to change
    // this method will produce a string that respects the \r and \n in the data value
    public static string ToDataString(this XElement xml)
    {
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            NewLineHandling = NewLineHandling.None,
            Indent = false,
        };
        var output = new StringBuilder();
        using (var writer = XmlWriter.Create(output, settings))
        {
            xml.WriteTo(writer);
        }

        return output.ToString();
    }
}
