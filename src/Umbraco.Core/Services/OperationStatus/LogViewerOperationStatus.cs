namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a log viewer operation.
/// </summary>
public enum LogViewerOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified saved log search was not found.
    /// </summary>
    NotFoundLogSearch,

    /// <summary>
    ///     A saved log search with the same name already exists.
    /// </summary>
    DuplicateLogSearch,

    /// <summary>
    ///     The operation was cancelled because the log file size exceeds the allowed limit.
    /// </summary>
    CancelledByLogsSizeValidation
}
