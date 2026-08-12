namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides the absolute expiry of the current back-office session.
/// </summary>
public interface ISessionExpiryAccessor
{
    /// <summary>
    ///     Gets the absolute UTC time at which the current session expires.
    /// </summary>
    /// <returns>
    ///     The expiry, or <c>null</c> when there is no ambient session, or the session carries no
    ///     readable expiry - for example a request authenticated by an API token rather than the
    ///     back-office cookie.
    /// </returns>
    DateTimeOffset? GetSessionExpiry();
}
