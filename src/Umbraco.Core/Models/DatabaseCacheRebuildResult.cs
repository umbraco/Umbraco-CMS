namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents the result of a database cache rebuild operation.
/// </summary>
public enum DatabaseCacheRebuildResult
{
    /// <summary>
    /// The cache rebuild operation was either successful or enqueued successfully.
    /// </summary>
    Success,

    /// <summary>
    /// A cache rebuild operation is already in progress.
    /// </summary>
    AlreadyRunning,
}
