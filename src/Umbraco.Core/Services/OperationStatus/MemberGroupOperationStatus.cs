namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a member group operation.
/// </summary>
public enum MemberGroupOperationStatus
{
    /// <summary>
    /// The member group operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The specified member group was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The member group name cannot be empty.
    /// </summary>
    CannotHaveEmptyName,

    /// <summary>
    /// The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    /// A member group with the same name already exists.
    /// </summary>
    DuplicateName,

    /// <summary>
    /// A member group with the same key already exists.
    /// </summary>
    DuplicateKey,
}
