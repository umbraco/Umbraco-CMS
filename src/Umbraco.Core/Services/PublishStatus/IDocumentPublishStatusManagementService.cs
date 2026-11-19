namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Provides management operations for the in-memory document publish status cache.
/// </summary>
public interface IDocumentPublishStatusManagementService
{
    /// <summary>
    /// Initializes the document publish status cache by loading all published document statuses from the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates the publish status for a specific document in the cache.
    /// </summary>
    /// <param name="documentKey">The unique key of the document to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddOrUpdateStatusAsync(Guid documentKey, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a document from the publish status cache.
    /// </summary>
    /// <param name="documentKey">The unique key of the document to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(Guid documentKey, CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates the publish status for a document and all its descendants in the cache.
    /// </summary>
    /// <param name="rootDocumentKey">The unique key of the root document.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddOrUpdateStatusWithDescendantsAsync(Guid rootDocumentKey, CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates the publish status for a specific document in the cache without leaving the calling thread.
    /// </summary>
    /// <param name="documentKey">The unique key of the document to update.</param>
    /// <remarks>
    /// Called by the content cache refresher on the saving thread after the commit. Until the lock wait and the
    /// notification pipeline are asynchronous, awaiting there needs a second thread-pool thread while the first is
    /// blocked, which under load delays the whole post-commit refresh.
    /// </remarks>
    // TODO (V20): Remove the default implementation once locks and notification publishing are asynchronous.
    void AddOrUpdateStatus(Guid documentKey)
        => AddOrUpdateStatusAsync(documentKey, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Adds or updates the publish status for a document and all its descendants without leaving the calling thread.
    /// </summary>
    /// <param name="rootDocumentKey">The unique key of the root document.</param>
    // TODO (V20): Remove the default implementation once locks and notification publishing are asynchronous.
    void AddOrUpdateStatusWithDescendants(Guid rootDocumentKey)
        => AddOrUpdateStatusWithDescendantsAsync(rootDocumentKey, CancellationToken.None).GetAwaiter().GetResult();
}
