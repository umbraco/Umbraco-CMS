namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a language operation.
/// </summary>
public enum LanguageOperationStatus
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
    ///     The fallback language configuration is invalid.
    /// </summary>
    InvalidFallback,

    /// <summary>
    ///     The specified language was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The default language is missing and cannot be removed.
    /// </summary>
    MissingDefault,

    /// <summary>
    ///     A language with the same ISO code already exists.
    /// </summary>
    DuplicateIsoCode,

    /// <summary>
    ///     The ISO code is invalid or not recognized.
    /// </summary>
    InvalidIsoCode,

    /// <summary>
    ///     The fallback language ISO code is invalid or not recognized.
    /// </summary>
    InvalidFallbackIsoCode,

    /// <summary>
    ///     The language ID is invalid.
    /// </summary>
    InvalidId
}
