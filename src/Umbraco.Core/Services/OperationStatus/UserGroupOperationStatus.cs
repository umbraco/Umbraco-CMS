namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum UserGroupOperationStatus
{
    Success,
    NotFound,
    UserNotFound,
    AlreadyExists,
    DuplicateAlias,
    MissingUser,
    CanNotDeleteIsSystemUserGroup,
    CanNotUpdateAliasIsSystemUserGroup,
    CancelledByNotification,
    MediaStartNodeKeyNotFound,
    DocumentStartNodeKeyNotFound,
    DocumentPermissionKeyNotFound,
    DocumentTypePermissionKeyNotFound,
    LanguageNotFound,
    NameTooLong,
    AliasTooLong,
    MissingName,
    Unauthorized,
    AdminGroupCannotBeEmpty,
    UserNotInGroup,
}
