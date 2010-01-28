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
		public static XmlNode ImportXmlNodeFromText (string text, ref XmlDocument xmlDoc) 
		{
			xmlDoc.LoadXml(text);
			return xmlDoc.FirstChild;
		}

        /// <summary>
        /// Opens a file as a XmlDocument.
        /// </summary>
        /// <param name="filePath">The relative file path. ei. /config/umbraco.config</param>
        /// <returns>Returns a XmlDocument class</returns>
        public static XmlDocument OpenAsXmlDocument(string filePath) {

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
			if (n == null || n.FirstChild == null)
				return string.Empty;
			return n.FirstChild.Value;
		}
	}
}
