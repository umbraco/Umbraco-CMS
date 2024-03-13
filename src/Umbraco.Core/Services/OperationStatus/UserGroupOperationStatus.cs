namespace Umbraco.Cms.Core.Services.OperationStatus;

// FIXME: Move all authorization statuses to <see cref="UserGroupAuthorizationStatus"/>
public enum UserGroupOperationStatus
{
    Success,
    NotFound,
    UserNotFound,
    AlreadyExists,
    DuplicateAlias,
    MissingUser,
    IsSystemUserGroup,
    CancelledByNotification,
    MediaStartNodeKeyNotFound,
    DocumentStartNodeKeyNotFound,
    DocumentPermissionKeyNotFound,
    LanguageNotFound,
    NameTooLong,
    AliasTooLong,
    MissingName,
    UnauthorizedMissingAllowedSectionAccess,
    UnauthorizedMissingContentStartNodeAccess,
    UnauthorizedMissingMediaStartNodeAccess,
    UnauthorizedMissingUserGroupAccess,
    UnauthorizedMissingUsersSectionAccess,
    AdminGroupCannotBeEmpty,
    UserNotInGroup,
}
