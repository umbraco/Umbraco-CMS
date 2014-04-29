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
            var xmlSerializer = new EntityXmlSerializer();
            return xmlSerializer.Serialize(ApplicationContext.Current.Services.DataTypeService, property);
        }
        
    }
}