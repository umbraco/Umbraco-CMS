﻿namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum UserOperationStatus
{
    Success,
    MissingUser,
    MissingUserGroup,
    UserNameIsNotEmail,
    EmailCannotBeChanged,
    NoUserGroup,
    DuplicateUserName,
    InvalidEmail,
    DuplicateEmail,
    Unauthorized,
    Forbidden,
    CancelledByNotification,
    UserNotFound,
    AvatarFileNotFound,
    CannotInvite,
    CannotDelete,
    CannotDisableSelf,
    CannotDisableInvitedUser,
    OldPasswordRequired,
    InvalidAvatar,
    InvalidIsoCode,
    ContentStartNodeNotFound,
    MediaStartNodeNotFound,
    UnknownFailure,
}
