namespace Umbraco.Cms.Core.Services.OperationStatus;

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
    Unauthorized,
    AdminGroupCannotBeEmpty,
    UserNotInGroup,
}
