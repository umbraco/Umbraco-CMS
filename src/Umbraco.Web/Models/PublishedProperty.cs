using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        internal static IEnumerable<IPublishedProperty> MapProperties(
            IEnumerable<PublishedPropertyType> propertyTypes, IEnumerable<Property> properties,
            Func<PublishedPropertyType, Property, object, IPublishedProperty> map)
        {
            var peResolver = PropertyEditorResolver.Current;
            var dtService = ApplicationContext.Current.Services.DataTypeService;
            return MapProperties(propertyTypes, properties, peResolver, dtService, map);
        }

        /// <summary>
        /// Maps a collection of Property to a collection of IPublishedProperty for a specified collection of PublishedPropertyType.
        /// </summary>
        /// <param name="propertyTypes">The published property types.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="map">A mapping function.</param>
        /// <param name="propertyEditorResolver">A PropertyEditorResolver instance.</param>
        /// <param name="dataTypeService">An IDataTypeService instance.</param>
        /// <returns>A collection of IPublishedProperty corresponding to the collection of PublishedPropertyType
        /// and taking values from the collection of Property.</returns>
        /// <remarks>Ensures that all conversions took place correctly.</remarks>
        internal static IEnumerable<IPublishedProperty> MapProperties(
            IEnumerable<PublishedPropertyType> propertyTypes, IEnumerable<Property> properties,
            PropertyEditorResolver propertyEditorResolver, IDataTypeService dataTypeService,
            Func<PublishedPropertyType, Property, object, IPublishedProperty> map)
        {
            return propertyTypes
                .Select(x =>
                {
                    var p = properties.SingleOrDefault(xx => xx.Alias == x.PropertyTypeAlias);
                    var v = p == null || p.Value == null ? null : p.Value;
                    if (v != null)
                    {
                        var e = propertyEditorResolver.GetByAlias(x.PropertyEditorAlias);
                        if (e != null)
                            v = e.ValueEditor.ConvertDbToString(p, p.PropertyType, dataTypeService);
                    }
                    // fixme - means that the IPropertyValueConverter will always get a string
                    // fixme   and never an int or DateTime that's in the DB unless the value editor has
                    // fixme   a way to say it's OK to use what's in the DB?

                    return map(x, p, v);
                });
        }
    }
}
