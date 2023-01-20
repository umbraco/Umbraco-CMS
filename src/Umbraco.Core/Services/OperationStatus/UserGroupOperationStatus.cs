namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum UserGroupOperationStatus
{
    Success,
    NotFound,
    AlreadyExists,
    DuplicateAlias,
    MissingUser,
    UnauthorizedMissingSections,
    UnauthorizedStartNodes,
    CancelledByNotification
}
