using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides published content properties converter meta data.
    /// </summary>
    public interface IPropertyValueConverterMeta : IPropertyValueConverter
    {
        /// <summary>
        /// Gets the type of values returned by the converter.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>The CLR type of values returned by the converter.</returns>
        Type GetPropertyValueType(PublishedPropertyType propertyType);

        /// <summary>
        /// Gets the property cache level of a specified value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="cacheValue">The property value.</param>
        /// <returns>The property cache level of the specified value.</returns>
        PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue);
    }
}
