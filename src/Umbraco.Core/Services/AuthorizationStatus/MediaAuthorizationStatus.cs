namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

public enum MediaAuthorizationStatus
{
    Success,
    NotFound,
    UnauthorizedMissingBinAccess,
    UnauthorizedMissingPathAccess,
    UnauthorizedMissingRootAccess
}
