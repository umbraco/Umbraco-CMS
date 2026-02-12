namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a member editing operation.
/// </summary>
public enum MemberEditingOperationStatus
{
    /// <summary>
    /// The member editing operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The specified member was not found.
    /// </summary>
    MemberNotFound,

    /// <summary>
    /// The specified member type was not found.
    /// </summary>
    MemberTypeNotFound,

    /// <summary>
    /// Failed to unlock the member account.
    /// </summary>
    UnlockFailed,

    /// <summary>
    /// Failed to disable two-factor authentication for the member.
    /// </summary>
    DisableTwoFactorFailed,

    /// <summary>
    /// Failed to assign or remove roles for the member.
    /// </summary>
    RoleAssignmentFailed,

    /// <summary>
    /// Failed to change the member's password.
    /// </summary>
    PasswordChangeFailed,

    /// <summary>
    /// The provided password does not meet the password requirements.
    /// </summary>
    InvalidPassword,

    /// <summary>
    /// The provided member name is invalid or empty.
    /// </summary>
    InvalidName,

    /// <summary>
    /// The provided username is invalid or empty.
    /// </summary>
    InvalidUsername,

    /// <summary>
    /// The provided email address is invalid or empty.
    /// </summary>
    InvalidEmail,

    /// <summary>
    /// A member with the same username already exists.
    /// </summary>
    DuplicateUsername,

    /// <summary>
    /// A member with the same email address already exists.
    /// </summary>
    DuplicateEmail,

    /// <summary>
    /// The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotificationHandler,

    /// <summary>
    /// An unknown error occurred during the operation.
    /// </summary>
    Unknown,
}
