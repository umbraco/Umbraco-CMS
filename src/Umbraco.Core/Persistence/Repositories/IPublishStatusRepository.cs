namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for querying document publish status.
/// </summary>
public interface IPublishStatusRepository
{
    /// <summary>
    ///     Gets the publish status for all documents.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A dictionary mapping document keys to their published culture codes.</returns>
    Task<IDictionary<Guid, ISet<string>>> GetAllPublishStatusAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the publish status for a specific document.
    /// </summary>
    /// <param name="documentKey">The unique key of the document.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A set of culture codes for which the document is published.</returns>
    Task<ISet<string>> GetPublishStatusAsync(Guid documentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the publish status for a document and all its descendants.
    /// </summary>
    /// <param name="rootDocumentKey">The unique key of the root document.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A dictionary mapping document keys to their published culture codes.</returns>
    Task<IDictionary<Guid, ISet<string>>> GetDescendantsOrSelfPublishStatusAsync(Guid rootDocumentKey, CancellationToken cancellationToken);
}
