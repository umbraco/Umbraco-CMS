namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a relation type operation.
/// </summary>
public enum RelationTypeOperationStatus
{
    /// <summary>
    /// The relation type operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The specified relation type was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// A relation type with the same key already exists.
    /// </summary>
    KeyAlreadyExists,

    /// <summary>
    /// The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    /// The provided relation type identifier is invalid.
    /// </summary>
    InvalidId,

    /// <summary>
    /// The specified child object type is invalid for this relation type.
    /// </summary>
    InvalidChildObjectType,

    /// <summary>
    /// The specified parent object type is invalid for this relation type.
    /// </summary>
    InvalidParentObjectType,
}
