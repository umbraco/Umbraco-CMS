namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum DomainOperationStatus
{
    Success,
    CancelledByNotification,
    ContentNotFound,
    LanguageNotFound,
    DuplicateDomainName,
    ConflictingDomainName,
    InvalidDomainName
}
