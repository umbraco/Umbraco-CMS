namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Provides management operations for the in-memory publish status cache.
/// </summary>
/// <remarks>
/// This service is responsible for maintaining the publish status cache that tracks
/// which documents are published and in which cultures.
/// </remarks>
public interface IPublishStatusManagementService
{
    /// <summary>
    /// Initializes the publish status cache by loading all published document statuses from the database.
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
}
