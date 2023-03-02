namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum UserOperationStatus
{
    Success,
    MissingUser,
    MissingUserGroup,
    UserNameIsNotEmail,
    DuplicateUserName,
    DuplicateEmail,
}
