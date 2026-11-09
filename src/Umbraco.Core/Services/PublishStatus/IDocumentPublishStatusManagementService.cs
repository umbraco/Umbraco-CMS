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
    /// Brings the cached publish status for a single culture in line with the database.
    /// </summary>
    /// <remarks>
    /// The culture is added to every document published in it and removed from every other document. Used when a
    /// language is added, removed or has its ISO code changed, so the cached status for that culture is brought up
    /// to date without reloading the status of every other culture.
    /// </remarks>
    /// <param name="culture">The culture code.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    // TODO (V20): Remove the default implementation.
    Task RefreshCultureAsync(string culture, CancellationToken cancellationToken)
        => InitializeAsync(cancellationToken);

    /// <summary>
    /// Ensures the default language used to resolve publish status queries made without a culture is re-read before
    /// it is next used.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    // TODO (V20): Remove the default implementation.
    // This runs for every language change, so the default leaves existing implementations as they were rather than
    // reloading the whole cache each time.
    Task RefreshDefaultCultureAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
