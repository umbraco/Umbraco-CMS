// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for xml objects
/// </summary>
public static class XmlExtensions
{
    /// <summary>
    /// Determines whether the attribute collection contains an attribute with the specified name.
    /// </summary>
    /// <param name="attributes">The XML attribute collection.</param>
    /// <param name="attributeName">The name of the attribute to check for.</param>
    /// <returns><c>true</c> if the collection contains the attribute; otherwise, <c>false</c>.</returns>
    public static bool HasAttribute(this XmlAttributeCollection attributes, string attributeName) =>
        attributes.Cast<XmlAttribute>().Any(x => x.Name == attributeName);

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

    /// <summary>
    ///     Converts from an <see cref="XContainer"/> to an <see cref="XmlNode"/>.
    /// </summary>
    /// <param name="xElement">The XContainer to convert.</param>
    /// <returns>The document element as an XmlNode, or null if the document has no root element.</returns>
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

    /// <summary>
    /// Gets the required attribute value from an XML element, converted to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the attribute value to.</typeparam>
    /// <param name="xml">The XML element.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute value converted to the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the xml parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the attribute is not found or cannot be converted.</exception>
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

    /// <summary>
    /// Gets an attribute value from an XML element, converted to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the attribute value to.</typeparam>
    /// <param name="xml">The XML element.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute value converted to the specified type, or the default value if not found or conversion fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the xml parameter is null.</exception>
    public static T? AttributeValue<T>(this XElement xml, string attributeName)
    {
        if (xml == null)
        {
            throw new ArgumentNullException("xml");
        }

        XAttribute? xmlAttribute = xml.Attribute(attributeName);
        if (xmlAttribute == null)
        {
            return default;
        }

        Attempt<T> result = xmlAttribute.Value.TryConvertTo<T>();
        if (result.Success)
        {
            return result.Result;
        }

        return default;
    }

    /// <summary>
    /// Gets an attribute value from an XML node, converted to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the attribute value to.</typeparam>
    /// <param name="xml">The XML node.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute value converted to the specified type, or the default value if not found or conversion fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the xml parameter is null.</exception>
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

        XmlAttribute? xmlAttribute = xml.Attributes[attributeName];
        if (xmlAttribute == null)
        {
            return default;
        }

        Attempt<T> result = xmlAttribute.Value.TryConvertTo<T>();
        if (result.Success)
        {
            return result.Result;
        }

        return default;
    }

    /// <summary>
    /// Converts an <see cref="XmlNode"/> to an <see cref="XElement"/>.
    /// </summary>
    /// <param name="node">The XML node to convert.</param>
    /// <returns>The converted XElement, or null if the node has no root element.</returns>
    public static XElement? GetXElement(this XmlNode node)
    {
        var xDoc = new XDocument();
        using (XmlWriter xmlWriter = xDoc.CreateWriter())
        {
            node.WriteTo(xmlWriter);
        }

        return xDoc.Root;
    }

    /// <summary>
    /// Converts an <see cref="XContainer"/> to an <see cref="XmlNode"/>.
    /// </summary>
    /// <param name="element">The XContainer to convert.</param>
    /// <returns>The converted XmlNode, or null if conversion fails.</returns>
    public static XmlNode? GetXmlNode(this XContainer element)
    {
        using (XmlReader xmlReader = element.CreateReader())
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlReader);
            return xmlDoc.DocumentElement;
        }
    }

    /// <summary>
    /// Converts an <see cref="XContainer"/> to an <see cref="XmlNode"/> and imports it into the specified document.
    /// </summary>
    /// <param name="element">The XContainer to convert.</param>
    /// <param name="xmlDoc">The XML document to import the node into.</param>
    /// <returns>The imported XmlNode, or null if conversion fails.</returns>
    public static XmlNode? GetXmlNode(this XContainer element, XmlDocument xmlDoc)
    {
        XmlNode? node = element.GetXmlNode();
        if (node is not null)
        {
            return xmlDoc.ImportNode(node, true);
        }

        return null;
    }

    /// <summary>
    ///     Converts an <see cref="XElement"/> to a string while preserving the exact line endings in text values.
    /// </summary>
    /// <param name="xml">The XML element to convert.</param>
    /// <returns>A string representation of the XML element with preserved line endings.</returns>
    /// <remarks>
    ///     This method exists because <see cref="XElement.ToString()"/> with SaveOptions changes line endings.
    ///     When saving data, we want the exact characters to be preserved.
    /// </remarks>
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
