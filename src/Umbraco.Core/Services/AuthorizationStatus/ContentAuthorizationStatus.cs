namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

public enum ContentAuthorizationStatus
{
    Success,
    NotFound,
    UnauthorizedMissingBinAccess,
    UnauthorizedMissingDescendantAccess,
    UnauthorizedMissingPathAccess,
    UnauthorizedMissingRootAccess,
    UnauthorizedMissingCulture,
    UnauthorizedMissingPermissionAccess
}
