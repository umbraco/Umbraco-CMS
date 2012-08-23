using System;
using System.Xml;
using Umbraco.Core.IO;

namespace Umbraco.Core
{
	/// <summary>
	/// The XmlHelper class contains general helper methods for working with xml in umbraco.
    /// </summary>
    internal class XmlHelper
    {

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
            if (!string.IsNullOrEmpty(xml))
            {
                xml = xml.Trim();

                if (xml.StartsWith("<") && xml.EndsWith(">"))
                {
                    return true;
                }
            }

            return false;
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
    }
}
