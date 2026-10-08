namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Provides management operations for the in-memory element publish status cache.
/// </summary>
public interface IElementPublishStatusManagementService
{
    /// <summary>
    /// Initializes the element publish status cache by loading all published element statuses from the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates the publish status for a specific element in the cache.
    /// </summary>
    /// <param name="elementKey">The unique key of the element to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddOrUpdateStatusAsync(Guid elementKey, CancellationToken cancellationToken);

    /// <summary>
    /// Removes an element from the publish status cache.
    /// </summary>
    /// <param name="elementKey">The unique key of the element to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(Guid elementKey, CancellationToken cancellationToken);
}
