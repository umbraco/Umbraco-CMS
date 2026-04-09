namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The result returned from the IBackOfficeUserPasswordChecker.
/// </summary>
public enum BackOfficeUserPasswordCheckerResult
{
    /// <summary>
    ///     The credentials are valid.
    /// </summary>
    ValidCredentials,

    /// <summary>
    ///     The credentials are invalid.
    /// </summary>
    InvalidCredentials,

    /// <summary>
    ///     The checker cannot validate the credentials and should fall back to the default checker.
    /// </summary>
    FallbackToDefaultChecker,
}
