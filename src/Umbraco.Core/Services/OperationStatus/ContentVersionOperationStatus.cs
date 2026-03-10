namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content version operation.
/// </summary>
public enum ContentVersionOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified content version was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    ContentNotFound,

    /// <summary>
    ///     The skip or take parameters are invalid for pagination.
    /// </summary>
    InvalidSkipTake,

    /// <summary>
    ///     The rollback operation failed.
    /// </summary>
    RollBackFailed,

    /// <summary>
    ///     The rollback operation was canceled by a notification handler.
    /// </summary>
    RollBackCanceled
}
