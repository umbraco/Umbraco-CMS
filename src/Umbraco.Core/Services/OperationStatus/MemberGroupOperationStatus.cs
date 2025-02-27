namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum MemberGroupOperationStatus
{
    Success,
    NotFound,
    CannotHaveEmptyName,
    CancelledByNotification,
    DuplicateName,
    DuplicateKey,
}
