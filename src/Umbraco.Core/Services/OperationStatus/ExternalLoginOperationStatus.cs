namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ExternalLoginOperationStatus
{
    Success,
    UserNotFound,
    Unknown,
    IdentityNotFound,
    AuthenticationOptionsNotFound,
    UnlinkingDisabled,
    InvalidProviderKey,
    AuthenticationSchemeNotFound,
    Unauthorized,
    ExternalInfoNotFound,
    IdentityFailure,
    UserSecretNotFound,
    InvalidSecret
}
