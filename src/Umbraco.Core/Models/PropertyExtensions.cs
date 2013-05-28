using System;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;

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
            var xmlNode = xd.CreateNode(XmlNodeType.Element, nodeName, "");

            //Add the property alias to the legacy schema
            if (UmbracoSettings.UseLegacyXmlSchema)
            {
                var alias = xd.CreateAttribute("alias");
                alias.Value = property.Alias.ToSafeAlias();
                xmlNode.Attributes.Append(alias);
            }

            //TODO: We'll need to clean this up eventually but for now here's what we're doing:
            // * Check if the property's DataType is assigned from a Property Editor or a legacy IDataType
            // * Get the XML result from the IDataType if there is one, otherwise just construct a simple
            //      XML construct from the value returned from the Property Editor.
            // More details discussed here: https://groups.google.com/forum/?fromgroups=#!topic/umbraco-dev/fieWZzHj7oY

            //var dataType = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(property.PropertyType.DataTypeDefinitionId);
            //if (dataType == null) throw new InvalidOperationException("No data type definition found with id " + property.PropertyType.DataTypeDefinitionId);

            var propertyEditor = PropertyEditorResolver.Current.GetById(property.PropertyType.DataTypeId);
            if (propertyEditor != null)
            {
                switch (property.PropertyType.DataTypeDatabaseType)
                {                    
                    case DataTypeDatabaseType.Nvarchar:                        
                    case DataTypeDatabaseType.Integer:
                        xmlNode.AppendChild(xd.CreateTextNode(property.Value.ToXmlString<string>()));
                        break;
                    case DataTypeDatabaseType.Ntext:
                        //put text in cdata
                        xmlNode.AppendChild(xd.CreateCDataSection(property.Value.ToXmlString<string>()));
                        break;
                    case DataTypeDatabaseType.Date:
                        //treat dates differently, output the format as xml format
                        if (property.Value == null)
                        {
                            xmlNode.AppendChild(xd.CreateTextNode(string.Empty));
                        }
                        else
                        {
                            var dt = (DateTime)property.Value;
                            xmlNode.AppendChild(xd.CreateTextNode(dt.ToXmlString<DateTime>()));
                        }                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                //NOTE: An exception will be thrown if this doesn't exist
                var legacyDataType = property.PropertyType.DataType(property.Id);
                xmlNode.AppendChild(legacyDataType.Data.ToXMl(xd));
            }

            var element = xmlNode.GetXElement();
            return element;
        }
    }
}