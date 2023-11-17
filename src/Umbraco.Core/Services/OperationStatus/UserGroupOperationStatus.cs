namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum UserGroupOperationStatus
{
    Success,
    NotFound,
    AlreadyExists,
    DuplicateAlias,
    MissingUser,
    IsSystemUserGroup,
    CancelledByNotification,
    MediaStartNodeKeyNotFound,
    DocumentStartNodeKeyNotFound,
    LanguageNotFound,
    NameTooLong,
    AliasTooLong,
    MissingName,
    UnauthorizedMissingAllowedSectionAccess,
    UnauthorizedMissingContentStartNodeAccess,
    UnauthorizedMissingMediaStartNodeAccess,
    UnauthorizedMissingUserGroupAccess,
    UnauthorizedMissingUsersSectionAccess
}
