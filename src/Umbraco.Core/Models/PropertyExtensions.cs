using System;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
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
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "data" : property.Alias.ToSafeAlias();

            var xd = new XmlDocument();
            var xmlNode = xd.CreateNode(XmlNodeType.Element, nodeName, "");

            //Add the property alias to the legacy schema
            if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                var alias = xd.CreateAttribute("alias");
                alias.Value = property.Alias.ToSafeAlias();
                xmlNode.Attributes.Append(alias);
            }

            // * Get the XML result from the property editor if there is one, otherwise just construct a simple
            //      XML construct from the value returned from the Property Editor.
            // More details discussed here: https://groups.google.com/forum/?fromgroups=#!topic/umbraco-dev/fieWZzHj7oY

            var propertyEditor = PropertyEditorResolver.Current.GetByAlias(property.PropertyType.PropertyEditorAlias);
            if (propertyEditor != null)
            {
                var cacheValue = propertyEditor.ValueEditor.ConvertDbToString(property);

                switch (property.PropertyType.DataTypeDatabaseType)
                {                                        
                    case DataTypeDatabaseType.Date:
                    case DataTypeDatabaseType.Integer:
                        xmlNode.AppendChild(xd.CreateTextNode(cacheValue.ToString()));    
                        break;
                    case DataTypeDatabaseType.Nvarchar:
                    case DataTypeDatabaseType.Ntext:
                        //put text in cdata
                        xmlNode.AppendChild(xd.CreateCDataSection(cacheValue.ToString()));
                        break;                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
           
            var element = xmlNode.GetXElement();
            return element;
        }
    }
}