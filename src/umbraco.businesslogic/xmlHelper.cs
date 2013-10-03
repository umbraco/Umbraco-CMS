using System;
using System.Xml;
using Umbraco.Core.IO;

namespace umbraco
{
    /// <summary>
    /// The xmlHelper class contains general helper methods for working with xml in umbraco.
    /// </summary>
    [Obsolete("Use Umbraco.Core.XmlHelper instead")]
    public class xmlHelper
    {
        /// <summary>
        /// Imports a XML node from text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="xmlDoc">The XML doc.</param>
        /// <returns></returns>
        public static XmlNode ImportXmlNodeFromText(string text, ref XmlDocument xmlDoc)
        {
        	return Umbraco.Core.XmlHelper.ImportXmlNodeFromText(text, ref xmlDoc);
        }

        /// <summary>
        /// Opens a file as a XmlDocument.
        /// </summary>
        /// <param name="filePath">The relative file path. ei. /config/umbraco.config</param>
        /// <returns>Returns a XmlDocument class</returns>
        public static XmlDocument OpenAsXmlDocument(string filePath)
        {
			return Umbraco.Core.XmlHelper.OpenAsXmlDocument(filePath);
        }

        /// <summary>
        /// creates a XmlAttribute with the specified name and value
        /// </summary>
        /// <param name="Xd">The xmldocument.</param>
        /// <param name="Name">The name of the attribute.</param>
        /// <param name="Value">The value of the attribute.</param>
        /// <returns>a XmlAttribute</returns>
        public static XmlAttribute addAttribute(XmlDocument Xd, string Name, string Value)
        {
			return Umbraco.Core.XmlHelper.AddAttribute(Xd, Name, Value);
        }

        /// <summary>
        /// Creates a text XmlNode with the specified name and value
        /// </summary>
        /// <param name="Xd">The xmldocument.</param>
        /// <param name="Name">The node name.</param>
        /// <param name="Value">The node value.</param>
        /// <returns>a XmlNode</returns>
        public static XmlNode addTextNode(XmlDocument Xd, string Name, string Value)
        {
			return Umbraco.Core.XmlHelper.AddTextNode(Xd, Name, Value);
        }

        /// <summary>
        /// Creates a cdata XmlNode with the specified name and value
        /// </summary>
        /// <param name="Xd">The xmldocument.</param>
        /// <param name="Name">The node name.</param>
        /// <param name="Value">The node value.</param>
        /// <returns>A XmlNode</returns>
        public static XmlNode addCDataNode(XmlDocument Xd, string Name, string Value)
        {
			return Umbraco.Core.XmlHelper.AddCDataNode(Xd, Name, Value);
        }

        /// <summary>
        /// Gets the value of a XmlNode
        /// </summary>
        /// <param name="n">The XmlNode.</param>
        /// <returns>the value as a string</returns>
        public static string GetNodeValue(XmlNode n)
        {
			return Umbraco.Core.XmlHelper.GetNodeValue(n);
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
			return Umbraco.Core.XmlHelper.CouldItBeXml(xml);
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
			return Umbraco.Core.XmlHelper.Split(data, separator, rootName, elementName);
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
			return Umbraco.Core.XmlHelper.Split(xml, data, separator, rootName, elementName);
        }
    }
}
