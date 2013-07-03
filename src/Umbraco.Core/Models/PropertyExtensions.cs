using System;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Models
{
    public static class PropertyExtensions
    {
        /// <summary>
        /// Creates the xml representation for the <see cref="Property"/> object
        /// </summary>
        /// <param name="property"><see cref="Property"/> to generate xml for</param>
        /// <returns>Xml of the property and its value</returns>
        public static XElement ToXml(this Property property)
        {
            string nodeName = UmbracoSettings.UseLegacyXmlSchema ? "data" : property.Alias.ToSafeAlias();

            var xd = new XmlDocument();
            XmlNode xmlNode = xd.CreateNode(XmlNodeType.Element, nodeName, "");

            //Add the property alias to the legacy schema
            if (UmbracoSettings.UseLegacyXmlSchema)
            {
                var alias = xd.CreateAttribute("alias");
                alias.Value = property.Alias.ToSafeAlias();
                xmlNode.Attributes.Append(alias);
            }

            //This seems to fail during testing 
            //SD: With the new null checks below, this shouldn't fail anymore.
            var dt = property.PropertyType.DataType(property.Id);
            if (dt != null && dt.Data != null)
            {
                xmlNode.AppendChild(dt.Data.ToXMl(xd));    
            }
            
            var element = xmlNode.GetXElement();
            return element;
        }
    }
}