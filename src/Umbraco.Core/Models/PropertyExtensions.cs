using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Services;
using umbraco.interfaces;

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
            return property.ToXml(ApplicationContext.Current.Services.DataTypeService);
        }

        internal static XElement ToXml(this Property property, IDataTypeService dataTypeService)
        {
            var nodeName = UmbracoSettings.UseLegacyXmlSchema ? "data" : property.Alias.ToSafeAlias();

            var xd = new XmlDocument();
            var xmlNode = xd.CreateNode(XmlNodeType.Element, nodeName, "");

            //Add the property alias to the legacy schema
            if (UmbracoSettings.UseLegacyXmlSchema)
            {
                var alias = xd.CreateAttribute("alias");
                alias.Value = property.Alias.ToSafeAlias();
                xmlNode.Attributes.Append(alias);
            }

            //This seems to fail during testing 
            //SD: With the new null checks below, this shouldn't fail anymore.
            var dt = property.PropertyType.DataType(property.Id, dataTypeService);
            if (dt != null && dt.Data != null)
            {
                //We've already got the value for the property so we're going to give it to the 
                // data type's data property so it doesn't go re-look up the value from the db again.
                var defaultData = dt.Data as IDataValueSetter;
                if (defaultData != null)
                {
                    defaultData.SetValue(property.Value, property.PropertyType.DataTypeDatabaseType.ToString());
                }

                xmlNode.AppendChild(dt.Data.ToXMl(xd));
            }

            var element = xmlNode.GetXElement();
            return element;
        }
    }
}