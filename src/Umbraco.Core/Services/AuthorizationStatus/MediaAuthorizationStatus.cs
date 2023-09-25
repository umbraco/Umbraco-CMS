namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

public enum MediaAuthorizationStatus
{
    Success,
    UnauthorizedMissingBinAccess,
    UnauthorizedMissingPathAccess,
    UnauthorizedMissingRootAccess
}
