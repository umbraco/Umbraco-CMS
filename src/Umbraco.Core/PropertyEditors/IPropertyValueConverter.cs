using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides published content properties conversion service.
    /// </summary>
    public interface IPropertyValueConverter
    {
        #region Data to Source

        /// <summary>
        /// Gets a value indicating whether the converter can convert from Data value to Source value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>A value indicating whether the converter can convert from Data value to Source value.</returns>
        bool IsDataToSourceConverter(PublishedPropertyType propertyType);

        /// <summary>
        /// Converts a property Data value to a Source value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="source">The data value.</param>
        /// <param name="preview">A value indicating whether conversion should take place in preview mode.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// fixme
        /// <para>The converter should know how to convert a <c>null</c> raw value into the default value for the property type.</para>
        /// <para>Raw values may come from the database or from the XML cache (thus being strings).</para>
        /// </remarks>
        object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview);

        #endregion

        #region Source to Object

        /// <summary>
        /// Gets a value indicating whether the converter can convert from Source value to Object value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>A value indicating whether the converter can convert from Source value to Object value.</returns>
        bool IsSourceToObjectConverter(PublishedPropertyType propertyType);

        /// <summary>
        /// Converts a property Source value to an Object value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="source">The source value.</param>
        /// <param name="preview">A value indicating whether conversion should take place in preview mode.</param>
        /// <returns>The result of the conversion.</returns>
        /// fixme
        /// <remarks>The converter should know how to convert a <c>null</c> source value into the default value for the property type.</remarks>
        object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview);

        #endregion

        #region Source to XPath

        /// <summary>
        /// Gets a value indicating whether the converter can convert from Source value to XPath value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>A value indicating whether the converter can convert from Source value to XPath value.</returns>
        bool IsSourceToXPathConverter(PublishedPropertyType propertyType);

        /// <summary>
        /// Converts a property Source value to an XPath value.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="source">The source value.</param>
        /// <param name="preview">A value indicating whether conversion should take place in preview mode.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// fixme
        /// <para>The converter should know how to convert a <c>null</c> source value into the default value for the property type.</para>
        /// <para>If successful, the result should be either <c>null</c>, a non-empty string, or an <c>XPathNavigator</c> instance.</para>
        /// </remarks>
        object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview);

        #endregion
    }
}
