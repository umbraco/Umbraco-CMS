namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a last synced operation.
/// </summary>
public enum LastSyncedOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because the target could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation failed because an ID was invalid.
    /// </summary>
    InvalidId,
}
