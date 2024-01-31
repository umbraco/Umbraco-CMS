namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum MemberGroupOperationStatus
{
    Success,
    CannotHaveEmptyName,
    CancelledByNotification,
    DuplicateName
}
