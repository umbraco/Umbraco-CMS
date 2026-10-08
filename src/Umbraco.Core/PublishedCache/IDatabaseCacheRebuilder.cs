using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Defines operations for rebuild of the published content cache in the database.
/// </summary>
public interface IDatabaseCacheRebuilder
{
    /// <summary>
    /// Indicates if the database cache is in the process of being rebuilt.
    /// </summary>
    /// <returns></returns>
    bool IsRebuilding() => false;

    /// <summary>
    /// Indicates if the database cache is in the process of being rebuilt.
    /// </summary>
    /// <returns></returns>
    Task<bool> IsRebuildingAsync() => Task.FromResult(IsRebuilding());

    /// <summary>
    /// Rebuilds the database cache, optionally using a background thread.
    /// </summary>
    /// <param name="useBackgroundThread">Flag indicating whether to use a background thread for the operation and immediately return to the caller.</param>
    /// <returns>An attempt indicating the result of the rebuild operation.</returns>
    Task<Attempt<DatabaseCacheRebuildResult>> RebuildAsync(bool useBackgroundThread);

    /// <summary>
    /// Rebuilds the database cache if the configured serializer has changed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RebuildDatabaseCacheIfSerializerChangedAsync();
}
