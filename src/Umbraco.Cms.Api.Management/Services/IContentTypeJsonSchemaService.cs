using System.Text.Json.Nodes;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Provides services for generating JSON Schema for content types.
/// </summary>
/// <remarks>
/// <para>
/// This service generates JSON Schema (draft 2020-12) that describes the structure of content
/// creation payloads for specific content types (documents, media, members).
/// </para>
/// <para>
/// The generated schemas reference data type schemas via external <c>$ref</c> URIs to the
/// <c>/umbraco/management/api/v1/data-type/{id}/schema</c> endpoint. Tooling should resolve
/// these references by making HTTP requests to the referenced endpoints.
/// </para>
/// </remarks>
public interface IContentTypeJsonSchemaService
{
    /// <summary>
    /// Gets a JSON Schema for creating/updating documents of a specific document type.
    /// </summary>
    /// <param name="key">The unique key of the document type.</param>
    /// <returns>
    /// A JSON Schema as a <see cref="JsonObject"/>, or <c>null</c> if the document type was not found.
    /// </returns>
    Task<JsonObject?> GetDocumentTypeSchemaAsync(Guid key);

    /// <summary>
    /// Gets a JSON Schema for creating/updating media of a specific media type.
    /// </summary>
    /// <param name="key">The unique key of the media type.</param>
    /// <returns>
    /// A JSON Schema as a <see cref="JsonObject"/>, or <c>null</c> if the media type was not found.
    /// </returns>
    Task<JsonObject?> GetMediaTypeSchemaAsync(Guid key);

    /// <summary>
    /// Gets a JSON Schema for creating/updating members of a specific member type.
    /// </summary>
    /// <param name="key">The unique key of the member type.</param>
    /// <returns>
    /// A JSON Schema as a <see cref="JsonObject"/>, or <c>null</c> if the member type was not found.
    /// </returns>
    Task<JsonObject?> GetMemberTypeSchemaAsync(Guid key);
}
