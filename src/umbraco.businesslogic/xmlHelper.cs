using System;
using System.Xml;
using umbraco.IO;

namespace umbraco
{
    /// <summary>
    /// The xmlHelper class contains general helper methods for working with xml in umbraco.
    /// </summary>
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

            XmlTextReader reader = new XmlTextReader(IOHelper.MapPath(filePath));

            reader.WhitespaceHandling = WhitespaceHandling.All;
            XmlDocument xmlDoc = new XmlDocument();
            //Load the file into the XmlDocument
            xmlDoc.Load(reader);
            //Close off the connection to the file.
            reader.Close();

            return xmlDoc;
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
            XmlAttribute temp = Xd.CreateAttribute(Name);
            temp.Value = Value;
            return temp;
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
            XmlNode temp = Xd.CreateNode(XmlNodeType.Element, Name, "");
            temp.AppendChild(Xd.CreateTextNode(Value));
            return temp;
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
            XmlNode temp = Xd.CreateNode(XmlNodeType.Element, Name, "");
            temp.AppendChild(Xd.CreateCDataSection(Value));
            return temp;
        }

        /// <summary>
        /// Gets the value of a XmlNode
        /// </summary>
        /// <param name="n">The XmlNode.</param>
        /// <returns>the value as a string</returns>
        public static string GetNodeValue(XmlNode n)
        {
            string value = string.Empty;
            if (n == null || n.FirstChild == null)
                return value;
            if (n.FirstChild.Value != null)
            {
                value = n.FirstChild.Value;
            }
            else
            {
                value = n.InnerXml;
            }
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
                    var xn = xmlHelper.addTextNode(xml, elementName, value);
                    xml.DocumentElement.AppendChild(xn);
                }
            }

            // return the XML node.
            return xml;
        }
    }
}
