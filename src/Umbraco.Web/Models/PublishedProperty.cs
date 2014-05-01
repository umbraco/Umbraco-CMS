using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models
{
    public static class PublishedProperty
    {
        /// <summary>
        /// Maps a collection of Property to a collection of IPublishedProperty for a specified collection of PublishedPropertyType.
        /// </summary>
        /// <param name="propertyTypes">The published property types.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="map">A mapping function.</param>
        /// <returns>A collection of IPublishedProperty corresponding to the collection of PublishedPropertyType
        /// and taking values from the collection of Property.</returns>
        /// <remarks>Ensures that all conversions took place correctly.</remarks>
        internal static IEnumerable<IPublishedContentProperty> MapProperties(
            IEnumerable<PublishedPropertyType> propertyTypes, IEnumerable<Property> properties,
            Func<PublishedPropertyType, object, IPublishedContentProperty> map)
        {
            return propertyTypes.Select(x =>
            {
                var property = properties.SingleOrDefault(xx => xx.Alias == x.PropertyTypeAlias);
                var value = property == null ? null : property.Value;
                return map(x, ConvertPropertyValueFromDbToData(x, value));
            });
        }

        /// <summary>
        /// Converts a database property value to a "data" value ie a value that we can pass to
        /// IPropertyValueConverter.
        /// </summary>
        /// <param name="propertyType">The published property type.</param>
        /// <param name="value">The value.</param>
        /// <returns>The converted value.</returns>
        internal static object ConvertPropertyValueFromDbToData(PublishedPropertyType propertyType, object value)
        {
            if (value == null) return null;

            // We are converting to string, even for database values which are integer or
            // DateTime, which is not optimum. Doing differently would require that we have a way to tell
            // whether the conversion to XML string changes something or not... which we don't, and we
            // don't want to implement it as PropertyValueEditor.ConvertDbToXml/String should die anyway.

            // Don't think about improving the situation here: this is a corner case and the real
            // thing to do is to get rig of PropertyValueEditor.ConvertDbToXml/String.

            // works but better use the new API
            //var dataTypeDefinition = global::umbraco.cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(propertyType.DataTypeId);
            //var dataType = dataTypeDefinition.DataType;

            // transient resolver will create a new object each time we call it
            // so it is safe to alter DataTypeDefinitionId, Data, etc.
            var dataType = DataTypesResolver.Current.GetById(propertyType.PropertyEditorGuid);
            if (dataType != null)
            {
                dataType.DataTypeDefinitionId = propertyType.DataTypeId; // required else conversion fails
                var data = dataType.Data;
                data.Value = value;
                var n = data.ToXMl(new XmlDocument());
                if (n.NodeType == XmlNodeType.CDATA || n.NodeType == XmlNodeType.Text)
                    value = n.InnerText;
                else if (n.NodeType == XmlNodeType.Element)
                    value = n.InnerXml;
                // assuming there are no other node types that we need to take care of
            }

            return value;
        }
    }
}
