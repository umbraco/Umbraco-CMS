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
        /// Creates a detached published property.
        /// </summary>
        /// <param name="propertyType">A published property type.</param>
        /// <param name="value">The property data raw value.</param>
        /// <param name="isPreviewing">A value indicating whether to evaluate the property value in previewing context.</param>
        /// <returns>A detached published property holding the value.</returns>
        internal static IPublishedProperty GetDetached(PublishedPropertyType propertyType, object value, bool isPreviewing = false)
        {
            if (propertyType.IsDetachedOrNested == false)
                throw new ArgumentException("Property type is neither detached nor nested.", "propertyType");
            var property = UmbracoContext.Current.ContentCache.InnerCache.CreateDetachedProperty(propertyType, value, isPreviewing);
            return property;
        }

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
