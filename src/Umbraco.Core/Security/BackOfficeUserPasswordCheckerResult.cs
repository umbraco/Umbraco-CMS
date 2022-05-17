namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The result returned from the IBackOfficeUserPasswordChecker
/// </summary>
public enum BackOfficeUserPasswordCheckerResult
{
    ValidCredentials,
    InvalidCredentials,
    FallbackToDefaultChecker,
}
