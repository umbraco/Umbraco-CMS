namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

public enum ElementAuthorizationStatus
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