namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum TwoFactorOperationStatus
{
    Success,
    ProviderAlreadySetup,
    ProviderNameNotFound,
    InvalidCode,
    UserNotFound
}
