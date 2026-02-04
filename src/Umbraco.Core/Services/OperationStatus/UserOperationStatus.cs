namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Used to signal a user operation succeeded or an atomic failure reason
/// </summary>
public enum UserOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because the specified user is missing.
    /// </summary>
    MissingUser,

    /// <summary>
    ///     The operation failed because the specified user group is missing.
    /// </summary>
    MissingUserGroup,

    /// <summary>
    ///     The operation failed because the username must be a valid email address.
    /// </summary>
    UserNameIsNotEmail,

    /// <summary>
    ///     The operation failed because the email address cannot be changed.
    /// </summary>
    EmailCannotBeChanged,

    /// <summary>
    ///     The operation failed because no user group was specified.
    /// </summary>
    NoUserGroup,

    /// <summary>
    ///     The operation failed because the admin user group must not be empty.
    /// </summary>
    AdminUserGroupMustNotBeEmpty,

    /// <summary>
    ///     The operation failed because a user with the same username already exists.
    /// </summary>
    DuplicateUserName,

    /// <summary>
    ///     The operation failed because the email address is invalid.
    /// </summary>
    InvalidEmail,

    /// <summary>
    ///     The operation failed because a user with the same email address already exists.
    /// </summary>
    DuplicateEmail,

    /// <summary>
    ///     The operation failed because the current user is not authorized to perform this action.
    /// </summary>
    Unauthorized,

    /// <summary>
    ///     The operation failed because access to the requested resource is forbidden.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The operation failed because the user could not be found.
    /// </summary>
    UserNotFound,

    /// <summary>
    ///     The operation failed because the avatar file could not be found.
    /// </summary>
    AvatarFileNotFound,

    /// <summary>
    ///     The operation failed because the user cannot be invited.
    /// </summary>
    CannotInvite,

    /// <summary>
    ///     The operation failed because the user cannot be deleted.
    /// </summary>
    CannotDelete,

    /// <summary>
    ///     The operation failed because the current user cannot disable themselves.
    /// </summary>
    CannotDisableSelf,

    /// <summary>
    ///     The operation failed because the current user cannot delete themselves.
    /// </summary>
    CannotDeleteSelf,

    /// <summary>
    ///     The operation failed because an invited user cannot be disabled.
    /// </summary>
    CannotDisableInvitedUser,

    /// <summary>
    ///     The operation failed because the old password is required when the user is changing their own password.
    /// </summary>
    SelfOldPasswordRequired,

    /// <summary>
    ///     The operation failed because the avatar file is invalid.
    /// </summary>
    InvalidAvatar,

    /// <summary>
    ///     The operation failed because the ISO code is invalid.
    /// </summary>
    InvalidIsoCode,

    /// <summary>
    ///     The operation failed because the invite token is invalid.
    /// </summary>
    InvalidInviteToken,

    /// <summary>
    ///     The operation failed because the password reset token is invalid.
    /// </summary>
    InvalidPasswordResetToken,

    /// <summary>
    ///     The operation failed because the content start node could not be found.
    /// </summary>
    ContentStartNodeNotFound,

    /// <summary>
    ///     The operation failed because the media start node could not be found.
    /// </summary>
    MediaStartNodeNotFound,

    /// <summary>
    ///     The operation failed because the content node could not be found.
    /// </summary>
    ContentNodeNotFound,

    /// <summary>
    ///     The operation failed because the media node could not be found.
    /// </summary>
    MediaNodeNotFound,

    /// <summary>
    ///     The operation failed because the specified node could not be found.
    /// </summary>
    NodeNotFound,

    /// <summary>
    ///     The operation failed due to an unknown failure.
    /// </summary>
    UnknownFailure,

    /// <summary>
    ///     The operation failed because password reset is not allowed for this user.
    /// </summary>
    CannotPasswordReset,

    /// <summary>
    ///     The operation failed because the user is not in an invite state.
    /// </summary>
    NotInInviteState,

    /// <summary>
    ///     The operation failed because users cannot reset their own password through this operation.
    /// </summary>
    SelfPasswordResetNotAllowed,

    /// <summary>
    ///     The operation failed because a user with the same ID already exists.
    /// </summary>
    DuplicateId,

    /// <summary>
    ///     The operation failed because the user type is invalid.
    /// </summary>
    InvalidUserType,

    /// <summary>
    ///     The operation failed because the username is invalid.
    /// </summary>
    InvalidUserName,
}
