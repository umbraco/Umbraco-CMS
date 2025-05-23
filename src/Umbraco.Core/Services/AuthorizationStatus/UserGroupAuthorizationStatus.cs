namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

public enum UserGroupAuthorizationStatus
{
    Success,
    UnauthorizedMissingAllowedSectionAccess,
    UnauthorizedMissingContentStartNodeAccess,
    UnauthorizedMissingMediaStartNodeAccess,
    UnauthorizedMissingUserGroupAccess,
    UnauthorizedMissingUsersSectionAccess
}
