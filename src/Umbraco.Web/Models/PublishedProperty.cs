using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

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
        internal static IEnumerable<IPublishedProperty> MapProperties(
            IEnumerable<PublishedPropertyType> propertyTypes, IEnumerable<Property> properties,
            Func<PublishedPropertyType, object, IPublishedProperty> map)
        {
            var propertyEditorResolver = PropertyEditorResolver.Current;
            var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

            return propertyTypes.Select(x =>
                {
                    var p = properties.SingleOrDefault(xx => xx.Alias == x.PropertyTypeAlias);
                    var v = p == null || p.Value == null ? null : p.Value;
                    if (v != null)
                    {
                        var e = propertyEditorResolver.GetByAlias(x.PropertyEditorAlias);

                        // We are converting to string, even for database values which are integer or
                        // DateTime, which is not optimum. Doing differently would require that we have a way to tell
                        // whether the conversion to XML string changes something or not... which we don't, and we
                        // don't want to implement it as PropertyValueEditor.ConvertDbToXml/String should die anyway.

                        // Don't think about improving the situation here: this is a corner case and the real
                        // thing to do is to get rig of PropertyValueEditor.ConvertDbToXml/String.

                        // Use ConvertDbToString to keep it simple, although everywhere we use ConvertDbToXml and
                        // nothing ensures that the two methods are consistent.

                        if (e != null)
                            v = e.ValueEditor.ConvertDbToString(p, p.PropertyType, dataTypeService);
                    }

                    return map(x, v);
                });
        }
    }
}
