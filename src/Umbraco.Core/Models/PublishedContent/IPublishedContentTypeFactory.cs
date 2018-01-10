namespace Umbraco. Core.Models.PublishedContent
{
    /// <summary>
    /// Creates published content types.
    /// </summary>
    public interface IPublishedContentTypeFactory
    {
        /// <summary>
        /// Creates a published content type.
        /// </summary>
        /// <param name="contentType">An content type.</param>
        /// <returns>A published content type corresponding to the item type and content type.</returns>
        PublishedContentType CreateContentType(IContentTypeComposition contentType);

        /// <summary>
        /// Creates a published property type.
        /// </summary>
        /// <param name="contentType">The published content type owning the property.</param>
        /// <param name="propertyType">A property type.</param>
        /// <remarks>Is used by <see cref="PublishedContentType"/> constructor to create property types.</remarks>
        PublishedPropertyType CreatePropertyType(PublishedContentType contentType, PropertyType propertyType);

        /// <summary>
        /// Creates a published property type.
        /// </summary>
        /// <param name="contentType">The published content type owning the property.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="dataTypeId">The datatype identifier.</param>
        /// <param name="propertyEditorAlias">The property editor alias.</param> FIXME derive from dataTypeId?
        /// <param name="variations">The variations.</param>
        /// <remarks>Is used by <see cref="PublishedContentType"/> constructor to create special property types.</remarks>
        PublishedPropertyType CreatePropertyType(PublishedContentType contentType, string propertyTypeAlias, int dataTypeId, string propertyEditorAlias, ContentVariation variations);

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
