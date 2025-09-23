using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Gets various references of document types, like documents, blocks, or other document types.
/// </summary>
public interface IContentTypeReferenceService
{
    /// <summary>
    ///     Gets docuiment keys of all documents using the given document type key.
    /// </summary>
    /// <returns></returns>
    Task<PagedModel<Guid>> GetReferencedDocumentKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take);

    /// <summary>
    ///     Gets all property type aliases.
    /// </summary>
    /// <returns></returns>
    Task<PagedModel<Guid>> GetReferencedDocumentTypeKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take);
}
