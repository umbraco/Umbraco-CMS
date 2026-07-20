namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content type structure operation.
/// </summary>
public enum ContentTypeStructureOperationStatus
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
    ///     The specified container was not found.
    /// </summary>
    ContainerNotFound,

    /// <summary>
    ///     The operation is not allowed due to path restrictions.
    /// </summary>
    NotAllowedByPath,

    /// <summary>
    ///     The specified content type was not found.
    /// </summary>
    NotFound
}
