namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Creates published content types.
/// </summary>
public interface IPublishedContentTypeFactory
{
    /// <summary>
    ///     Creates a published content type.
    /// </summary>
    /// <param name="contentType">An content type.</param>
    /// <returns>A published content type corresponding to the item type and content type.</returns>
    IPublishedContentType CreateContentType(IContentTypeComposition contentType);

    /// <summary>
    ///     Creates a published property type.
    /// </summary>
    /// <param name="contentType">The published content type owning the property.</param>
    /// <param name="propertyType">A property type.</param>
    /// <remarks>Is used by <see cref="PublishedContentType" /> constructor to create property types.</remarks>
    IPublishedPropertyType CreatePropertyType(IPublishedContentType contentType, IPropertyType propertyType);

    /// <summary>
    ///     Creates a published property type.
    /// </summary>
    /// <param name="contentType">The published content type owning the property.</param>
    /// <param name="propertyTypeAlias">The property type alias.</param>
    /// <param name="dataTypeId">The datatype identifier.</param>
    /// <param name="variations">The variations.</param>
    /// <remarks>Is used by <see cref="PublishedContentType" /> constructor to create special property types.</remarks>
    IPublishedPropertyType CreatePropertyType(
        IPublishedContentType contentType,
        string propertyTypeAlias,
        int dataTypeId,
        ContentVariation variations);

    /// <summary>
    ///     Creates a core (non-user) published property type.
    /// </summary>
    /// <param name="contentType">The published content type owning the property.</param>
    /// <param name="propertyTypeAlias">The property type alias.</param>
    /// <param name="dataTypeId">The datatype identifier.</param>
    /// <param name="variations">The variations.</param>
    /// <remarks>Is used by <see cref="PublishedContentType" /> constructor to create special property types.</remarks>
    IPublishedPropertyType CreateCorePropertyType(
        IPublishedContentType contentType,
        string propertyTypeAlias,
        int dataTypeId,
        ContentVariation variations);

    /// <summary>
    ///     Gets a published datatype.
    /// </summary>
    PublishedDataType GetDataType(int id);

    /// <summary>
    ///     Notifies the factory of datatype changes.
    /// </summary>
    /// <remarks>
    ///     <para>This is so the factory can flush its caches.</para>
    ///     <para>Invoked by the IPublishedSnapshotService.</para>
    /// </remarks>
    void NotifyDataTypeChanges(int[] ids);
}
