namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a domain operation.
/// </summary>
public enum DomainOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    ContentNotFound,

    /// <summary>
    ///     The specified language was not found.
    /// </summary>
    LanguageNotFound,

    /// <summary>
    ///     A domain with the same name already exists.
    /// </summary>
    DuplicateDomainName,

    /// <summary>
    ///     The domain name conflicts with an existing domain.
    /// </summary>
    ConflictingDomainName,

    /// <summary>
    ///     The domain name is invalid.
    /// </summary>
    InvalidDomainName
}
