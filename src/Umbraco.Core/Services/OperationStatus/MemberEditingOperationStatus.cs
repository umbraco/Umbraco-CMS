namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum MemberEditingOperationStatus
{
    Success,
    MemberNotFound,
    MemberTypeNotFound,
    UnlockFailed,
    DisableTwoFactorFailed,
    RoleAssignmentFailed,
    PasswordChangeFailed,
    InvalidPassword,
    InvalidName,
    InvalidUsername,
    InvalidEmail,
    DuplicateUsername,
    DuplicateEmail,
    CancelledByNotificationHandler,
    Unknown,
}
