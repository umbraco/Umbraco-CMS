using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Gets various references of document types, like documents, blocks, or other document types.
/// </summary>
public interface IContentTypeReferenceService
{
    /// <summary>
    ///     Gets document keys of all documents using the given document type key.
    /// </summary>
    /// <returns></returns>
    Task<PagedModel<Guid>> GetReferencedDocumentKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take);

    /// <summary>
    ///     Gets keys of all the document type inhereting from given document type key.
    /// </summary>
    /// <returns></returns>
    Task<PagedModel<Guid>> GetReferencedDocumentTypeKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take);

    /// <summary>
    ///     Gets data type keys that reference element types.
    /// </summary>
    /// <returns></returns>
    Task<PagedModel<Guid>> GetReferencedElementsFromDataTypesAsync(Guid key, CancellationToken cancellationToken, int skip, int take);
}
