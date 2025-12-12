using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service to get content type schema information for schema generation.
/// </summary>
public interface IContentTypeSchemaService
{
    /// <summary>
    /// Gets all available document types.
    /// </summary>
    /// <returns>A collection of document type schema information.</returns>
    public IReadOnlyCollection<ContentTypeSchemaInfo> GetDocumentTypes();

    /// <summary>
    /// Gets all available media types.
    /// </summary>
    /// <returns>A collection of media type schema information.</returns>
    public IReadOnlyCollection<ContentTypeSchemaInfo> GetMediaTypes();
}
