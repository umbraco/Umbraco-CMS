using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides published content properties conversion service.
    /// </summary>
    public interface IPropertyValueConverter
    {
        /// <summary>
        /// Gets a value indicating whether the converter supports a property type.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>A value indicating whether the converter supports a property type.</returns>
        bool IsConverter(PublishedPropertyType propertyType);

        /// <summary>
        /// Gets the type of values returned by the converter.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>The CLR type of values returned by the converter.</returns>
        Type GetPropertyValueType(PublishedPropertyType propertyType);

        /// <summary>
        /// Gets the property cache level.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>The property cache level.</returns>
        PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType);

        /// <summary>
        /// Converts a property source value to an intermediate value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="source">The source value.</param>
        /// <param name="preview">A value indicating whether conversion should take place in preview mode.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// <para>The converter should know how to convert a <c>null</c> source value, meaning that no
        /// value has been assigned to the property. The intermediate value can be <c>null</c>.</para>
        /// <para>With the XML cache, source values come from the XML cache and therefore are strings.</para>
        /// <para>With objects caches, source values would come from the database and therefore be either
        /// ints, DateTimes, decimals, or strings.</para>
        /// <para>The converter should be prepared to handle both situations.</para>
        /// <para>When source values are strings, the converter must handle empty strings, whitespace
        /// strings, and xml-whitespace strings appropriately, ie it should know whether to preserve
        /// whitespaces.</para>
        /// </remarks>
        object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview);

        /// <summary>
        /// Converts a property intermediate value to an Object value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="referenceCacheLevel">The reference cache level.</param>
        /// <param name="inter">The intermediate value.</param>
        /// <param name="preview">A value indicating whether conversion should take place in preview mode.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// <para>The converter should know how to convert a <c>null</c> intermediate value, or any intermediate value
        /// indicating that no value has been assigned to the property. It is up to the converter to determine
        /// what to return in that case: either <c>null</c>, or the default value...</para>
        /// <para>The <paramref name="referenceCacheLevel"/> is passed to the converter so that it can be, in turn,
        /// passed to eg a PublishedFragment constructor. It is used by the fragment and the properties to manage
        /// the cache levels of property values. It is not meant to be used by the converter.</para>
        /// </remarks>
        object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview);

        /// <summary>
        /// Converts a property intermediate value to an XPath value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="referenceCacheLevel">The reference cache level.</param>
        /// <param name="inter">The intermediate value.</param>
        /// <param name="preview">A value indicating whether conversion should take place in preview mode.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// <para>The converter should know how to convert a <c>null</c> intermediate value, or any intermediate value
        /// indicating that no value has been assigned to the property. It is up to the converter to determine
        /// what to return in that case: either <c>null</c>, or the default value...</para>
        /// <para>If successful, the result should be either <c>null</c>, a string, or an <c>XPathNavigator</c>
        /// instance. Whether an xml-whitespace string should be returned as <c>null</c> or litterally, is
        /// up to the converter.</para>
        /// <para>The converter may want to return an XML fragment that represent a part of the content tree,
        /// but should pay attention not to create infinite loops that would kill XPath and XSLT.</para>
        /// <para>The <paramref name="referenceCacheLevel"/> is passed to the converter so that it can be, in turn,
        /// passed to eg a PublishedFragment constructor. It is used by the fragment and the properties to manage
        /// the cache levels of property values. It is not meant to be used by the converter.</para>
        /// </remarks>
        object ConvertInterToXPath(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview);
    }
}
