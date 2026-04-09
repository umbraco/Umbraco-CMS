namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an entity container (folder) operation.
/// </summary>
public enum EntityContainerOperationStatus
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
    ///     The object type is invalid for container operations.
    /// </summary>
    InvalidObjectType,

    /// <summary>
    ///     The container ID is invalid.
    /// </summary>
    InvalidId,

    /// <summary>
    ///     A container with the same key already exists.
    /// </summary>
    DuplicateKey,

    /// <summary>
    ///     The specified container was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The specified parent container was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    ///     The container is not empty and cannot be deleted.
    /// </summary>
    NotEmpty,

    /// <summary>
    ///     A container with the same name already exists at this level.
    /// </summary>
    DuplicateName
}
