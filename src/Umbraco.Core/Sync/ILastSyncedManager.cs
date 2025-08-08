namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Handles saving and pruning of the LastSynced database table.
/// </summary>
public interface ILastSyncedManager
{
    /// <summary>
    /// Used to fetch the last synced internal ID from the database.
    /// </summary>
    /// <returns>The Internal ID from the database.</returns>
    Task<int?> GetLastSyncedInternalAsync();

    /// <summary>
    /// Used to fetch the last synced external ID from the database.
    /// </summary>
    /// <returns>The External ID from the database.</returns>
    Task<int?> GetLastSyncedExternalAsync();

    /// <summary>
    /// Used to save the last synced Internal ID to the Database.
    /// </summary>
    /// <param name="id">The last synced internal ID.</param>
    Task SaveLastSyncedInternalAsync(int id);

    /// <summary>
    /// Used to save the last synced External ID to the Database.
    /// </summary>
    /// <param name="id">The last synced external ID.</param>
    Task SaveLastSyncedExternalAsync(int id);

    /// <summary>
    /// Deletes entries older than the set parameter. This method also removes any entries where both
    /// IDs are higher than the lowest synced CacheInstruction ID.
    /// </summary>
    /// <param name="pruneDate">Any date entries in the DB before this parameter, will be removed from the Database.</param>
    Task DeleteOlderThanAsync(DateTime date);
}
