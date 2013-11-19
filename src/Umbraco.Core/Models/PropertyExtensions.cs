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
            return property.ToXml(property.PropertyType, ApplicationContext.Current.Services.DataTypeService);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="Property"/> object
        /// </summary>
        /// <param name="property"><see cref="Property"/> to generate xml for</param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns>Xml of the property and its value</returns>
        internal static XElement ToXml(this Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "data" : property.Alias.ToSafeAlias();

            var xElement = new XElement(nodeName);

            //Add the property alias to the legacy schema
            if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                var a = new XAttribute("alias", property.Alias.ToSafeAlias());
                xElement.Add(a);
            }

            //Get the property editor for thsi property and let it convert it to the xml structure
            var propertyEditor = PropertyEditorResolver.Current.GetByAlias(property.PropertyType.PropertyEditorAlias);
            if (propertyEditor != null)
            {
                var xmlValue = propertyEditor.ValueEditor.ConvertDbToXml(property, propertyType, dataTypeService);
                xElement.Add(xmlValue);
            }

            return xElement;
        }
    }
}