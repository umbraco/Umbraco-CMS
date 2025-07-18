namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents the status of a long-running operation.
/// </summary>
public enum LongRunningOperationStatus
{
    /// <summary>
    /// The operation has finished successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The operation has failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The operation has been queued.
    /// </summary>
    Enqueued,

    /// <summary>
    /// The operation is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The operation wasn't updated within the expected time frame and is considered stale.
    /// </summary>
    Stale,
}
