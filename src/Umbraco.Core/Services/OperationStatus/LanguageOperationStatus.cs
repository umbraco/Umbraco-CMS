namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum LanguageOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidFallback,
    NotFound,
    MissingDefault,
    DuplicateIsoCode,
    InvalidIsoCode,
    InvalidFallbackIsoCode,
    InvalidId
}
