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
    /// Rebuilds the database cache.
    /// </summary>
    [Obsolete("Use the overload with the useBackgroundThread parameter. Scheduled for removal in Umbraco 17.")]
    void Rebuild();

    /// <summary>
    /// Rebuilds the database cache, optionally using a background thread.
    /// </summary>
    /// <param name="useBackgroundThread">Flag indicating whether to use a background thread for the operation and immediately return to the caller.</param>
    [Obsolete("Use RebuildAsync instead. Scheduled for removal in Umbraco 18.")]
    void Rebuild(bool useBackgroundThread)
#pragma warning disable CS0618 // Type or member is obsolete
        => Rebuild();
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Rebuilds the database cache, optionally using a background thread.
    /// </summary>
    /// <param name="useBackgroundThread">Flag indicating whether to use a background thread for the operation and immediately return to the caller.</param>
    /// <returns>An attempt indicating the result of the rebuild operation.</returns>
    Task<Attempt<DatabaseCacheRebuildResult>> RebuildAsync(bool useBackgroundThread)
    {
        Rebuild(useBackgroundThread);
        return Task.FromResult(Attempt.Succeed(DatabaseCacheRebuildResult.Success));
    }

    /// <summary>
    /// Rebuids the database cache if the configured serializer has changed.
    /// </summary>
    void RebuildDatabaseCacheIfSerializerChanged();
}
