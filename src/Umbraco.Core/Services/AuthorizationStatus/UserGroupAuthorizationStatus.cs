namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

// FIXME: Move all authorization related statuses from <see cref="UserGroupOperationStatus"/> to here
public enum UserGroupAuthorizationStatus
{
    Success,
    UnauthorizedMissingUserGroup
}
