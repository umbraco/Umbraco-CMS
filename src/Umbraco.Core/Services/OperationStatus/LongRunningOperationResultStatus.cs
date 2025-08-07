namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of the result of a long-running operation.
/// </summary>
public enum LongRunningOperationResultStatus
{
    /// <summary>
    /// The operation result was successfully retrieved.
    /// </summary>
    Success,

    /// <summary>
    /// The operation was not found, possibly due to unknown type or ID or it was already deleted.
    /// </summary>
    OperationNotFound,

    /// <summary>
    /// The operation is still running and the result is not yet available.
    /// </summary>
    OperationPending,

    /// <summary>
    /// The operation has failed, and the result is not available.
    /// </summary>
    OperationFailed,
}
