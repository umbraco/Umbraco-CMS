namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the result of attempting to enqueue a long-running operation.
/// </summary>
public enum LongRunningOperationEnqueueStatus
{
    /// <summary>
    /// The operation was successfully enqueued and will be executed in the background.
    /// </summary>
    Success,

    /// <summary>
    /// The operation is already running.
    /// </summary>
    AlreadyRunning,
}
