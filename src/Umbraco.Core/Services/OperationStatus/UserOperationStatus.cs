namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum UserOperationStatus
{
    Success,
    MissingUser,
    MissingUserGroup,
    UserNameIsNotEmail,
    EmailCannotBeChanged,
    DuplicateUserName,
    DuplicateEmail,
    Unauthorized,
    CancelledByNotification,
    NotFound,
    CannotInvite,
    CannotDelete,
    CannotDisableSelf,
    CannotDisableInvitedUser,
    OldPasswordRequired,
    InvalidAvatar,
    UnknownFailure,
}
