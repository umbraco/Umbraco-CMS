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
            var nodeName = LegacyUmbracoSettings.UseLegacyXmlSchema ? "data" : property.Alias.ToSafeAlias();

            var xd = new XmlDocument();
            var xmlNode = xd.CreateNode(XmlNodeType.Element, nodeName, "");

            //Add the property alias to the legacy schema
            if (LegacyUmbracoSettings.UseLegacyXmlSchema)
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

            var propertyEditor = PropertyEditorResolver.Current.GetById(property.PropertyType.DataTypeId);
            if (propertyEditor != null)
            {
                var cacheValue = propertyEditor.ValueEditor.FormatValueForCache(property);

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
            else
            {
                //NOTE: An exception will be thrown if this doesn't exist
                var legacyDataType = property.PropertyType.DataType(property.Id, dataTypeService);

                //We've already got the value for the property so we're going to give it to the 
                // data type's data property so it doesn't go re-look up the value from the db again.
                var defaultData = legacyDataType.Data as IDataValueSetter;
                if (defaultData != null)
                {
                    defaultData.SetValue(property.Value, property.PropertyType.DataTypeDatabaseType.ToString());
                }
                xmlNode.AppendChild(legacyDataType.Data.ToXMl(xd));
            }

            var element = xmlNode.GetXElement();
            return element;
        }
    }
}