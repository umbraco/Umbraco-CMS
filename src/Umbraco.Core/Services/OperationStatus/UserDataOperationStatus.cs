namespace Umbraco.Cms.Core.Services.OperationStatus;

// FIXME: Move all authorization statuses to <see cref="UserGroupAuthorizationStatus"/>
public enum UserDataOperationStatus
{
    Success,
    NotFound,
    UserNotFound,
    AlreadyExists
}
