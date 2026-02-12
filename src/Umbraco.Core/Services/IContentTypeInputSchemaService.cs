using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service to get content type input schema information for programmatic content creation.
/// </summary>
/// <remarks>
/// <para>
/// This service provides the minimal schema information needed to input content values,
/// including property aliases, data type keys, and variation settings.
/// </para>
/// <para>
/// Property groups and inheritance information are not included as they are not relevant for input.
/// </para>
/// </remarks>
public interface IContentTypeInputSchemaService
{
    /// <summary>
    /// Gets input schemas for specific document types by their keys.
    /// </summary>
    /// <param name="keys">The unique identifiers of the document types to retrieve.</param>
    /// <returns>
    /// A collection containing the schemas for document types that were found.
    /// Returns an empty collection if none of the specified keys were found.
    /// </returns>
    Task<IReadOnlyCollection<ContentTypeInputSchema>> GetDocumentTypeSchemasAsync(IEnumerable<Guid> keys);

    /// <summary>
    /// Gets input schemas for specific media types by their keys.
    /// </summary>
    /// <param name="keys">The unique identifiers of the media types to retrieve.</param>
    /// <returns>
    /// A collection containing the schemas for media types that were found.
    /// Returns an empty collection if none of the specified keys were found.
    /// </returns>
    Task<IReadOnlyCollection<ContentTypeInputSchema>> GetMediaTypeSchemasAsync(IEnumerable<Guid> keys);

    /// <summary>
    /// Gets input schemas for specific member types by their keys.
    /// </summary>
    /// <param name="keys">The unique identifiers of the member types to retrieve.</param>
    /// <returns>
    /// A collection containing the schemas for member types that were found.
    /// Returns an empty collection if none of the specified keys were found.
    /// </returns>
    Task<IReadOnlyCollection<ContentTypeInputSchema>> GetMemberTypeSchemasAsync(IEnumerable<Guid> keys);
}
