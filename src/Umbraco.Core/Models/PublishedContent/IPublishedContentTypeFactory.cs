namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Creates published content types.
    /// </summary>
    public interface IPublishedContentTypeFactory
    {
        /// <summary>
        /// Creates a published content type.
        /// </summary>
        /// <param name="itemType">The item type.</param>
        /// <param name="contentType">An content type.</param>
        /// <returns>A published content type corresponding to the item type and content type.</returns>
        PublishedContentType CreateContentType(PublishedItemType itemType, IContentTypeComposition contentType);
        // fixme could we derive itemType from contentType?

        /// <summary>
        /// Creates a published property type.
        /// </summary>
        /// <param name="contentType">The published content type owning the property.</param>
        /// <param name="propertyType">A property type.</param>
        /// <returns>A published property type corresponding to the property type and owned by the published content type.</returns>
        PublishedPropertyType CreatePropertyType(PublishedContentType contentType, PropertyType propertyType);

        /// <summary>
        /// Creates a published property type.
        /// </summary>
        /// <param name="umbraco">A value indicating whether the property is created by Umbraco.</param>
        /// <returns>fixme xplain.</returns>
        PublishedPropertyType CreatePropertyType(string propertyTypeAlias, int dataTypeId, string editorAlias, bool umbraco = false);

        /// <summary>
        /// Creates a published data type.
        /// </summary>
        /// <param name="id">The data type identifier.</param>
        /// <param name="editorAlias">The data type editor alias.</param>
        /// <returns>A published data type with the specified properties.</returns>
        /// <remarks>Properties are assumed to be consistent and not checked.</remarks>
        PublishedDataType CreateDataType(int id, string editorAlias);
    }
}
