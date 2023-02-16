namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum UserGroupOperationStatus
{
    Success,
    NotFound,
    AlreadyExists,
    DuplicateAlias,
    MissingUser,
    IsSystemUserGroup,
    UnauthorizedMissingUserSection,
    UnauthorizedMissingSections,
    UnauthorizedStartNodes,
    UnauthorizedMissingUserGroup,
    CancelledByNotification,
    MediaStartNodeKeyNotFound,
    DocumentStartNodeKeyNotFound,
    LanguageNotFound,
    NameTooLong,
    AliasTooLong,
    MissingName,
}
