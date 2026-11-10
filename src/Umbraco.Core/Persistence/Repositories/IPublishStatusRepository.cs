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

    /// <summary>
    /// Gets the published cultures of a document without leaving the calling thread.
    /// </summary>
    /// <remarks>
    /// Used by the cache refresher on the saving thread; awaiting there needs a second thread-pool thread while
    /// the first is blocked, which under load delays the whole post-commit refresh.
    /// </remarks>
    /// <param name="documentKey">The key of the document.</param>
    /// <returns>The cultures in which the document is published; empty for invariant or unpublished documents.</returns>
    // TODO (V20): Remove the default implementation once locks and notification publishing are asynchronous.
    ISet<string> GetPublishStatus(Guid documentKey)
        => GetPublishStatusAsync(documentKey, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Gets the published cultures of a document and its descendants without leaving the calling thread.
    /// </summary>
    /// <param name="rootDocumentKey">The key of the document at the root of the subtree.</param>
    /// <returns>The published cultures of each document in the subtree, keyed by document key.</returns>
    // TODO (V20): Remove the default implementation once locks and notification publishing are asynchronous.
    IDictionary<Guid, ISet<string>> GetDescendantsOrSelfPublishStatus(Guid rootDocumentKey)
        => GetDescendantsOrSelfPublishStatusAsync(rootDocumentKey, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets the keys of all documents that are published in a culture.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The keys of the documents published in the culture, including invariant published documents.</returns>
    // TODO (V19): Remove the default implementation.
    async Task<IEnumerable<Guid>> GetPublishedKeysAsync(string culture, CancellationToken cancellationToken)
        => (await GetAllPublishStatusAsync(cancellationToken))
            .Where(x => x.Value.Contains(culture, StringComparer.InvariantCultureIgnoreCase))
            .Select(x => x.Key)
            .ToArray();
}
