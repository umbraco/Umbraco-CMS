namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a data type operation.
/// </summary>
public enum DataTypeOperationStatus
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
    ///     The data type configuration is invalid.
    /// </summary>
    InvalidConfiguration,

    /// <summary>
    ///     The data type name is invalid or empty.
    /// </summary>
    InvalidName,

    /// <summary>
    ///     The data type ID is invalid.
    /// </summary>
    InvalidId,

    /// <summary>
    ///     A data type with the same key already exists.
    /// </summary>
    DuplicateKey,

    /// <summary>
    ///     The specified data type was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The specified parent container was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    ///     The specified parent is not a valid container.
    /// </summary>
    ParentNotContainer,

    /// <summary>
    ///     The specified property editor was not found.
    /// </summary>
    PropertyEditorNotFound,

    /// <summary>
    ///     The data type cannot be deleted because it is a system data type or is in use.
    /// </summary>
    NonDeletable
}
