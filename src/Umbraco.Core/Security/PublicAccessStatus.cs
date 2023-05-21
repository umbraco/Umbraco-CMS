namespace Umbraco.Cms.Core.Security;

public enum PublicAccessStatus
{
    NotLoggedIn,
    AccessDenied,
    NotApproved,
    LockedOut,
    AccessAccepted,
}
