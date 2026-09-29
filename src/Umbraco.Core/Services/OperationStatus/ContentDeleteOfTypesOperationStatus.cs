namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> delete-content-of-types operation.
/// </summary>
public enum ContentDeleteOfTypesOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     One of the specified content types was not found.
    /// </summary>
    NotFound,
}
