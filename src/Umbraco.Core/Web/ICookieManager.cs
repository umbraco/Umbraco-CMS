namespace Umbraco.Cms.Core.Web;

/// <summary>
/// Defines cookie related operations.
/// </summary>
public interface ICookieManager
{
    /// <summary>
    /// Expires the cookie with the specified name.
    /// </summary>
    /// <param name="cookieName">The cookie name.</param>
    void ExpireCookie(string cookieName);

    /// <summary>
    /// Gets the value of the cookie with the specified name.
    /// </summary>
    /// <param name="cookieName">The cookie name.</param>
    string? GetCookieValue(string cookieName);

    /// <summary>
    /// Sets the value of a cookie with the specified name.
    /// </summary>
    /// <param name="cookieName">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="httpOnly">Indicates whether the created cookie should be marked as HTTP only.</param>
    /// <param name="secure">Indicates whether the created cookie should be marked as secure.</param>
    /// <param name="sameSiteMode">Indicates the created cookie's same site status.</param>
    /// <remarks>
    /// The value provided by <paramref name="sameSiteMode"/> should match the enum values available from
    /// Microsoft.AspNetCore.Http.SameSiteMode.
    /// This hasn't been used as the parameter directly to avoid a dependency on Microsoft.AspNetCore.Http in
    /// the core project.
    /// </remarks>
    [Obsolete("Please use the overload that accepts an expires parameter. This will be removed in Umbraco 19.")]
    void SetCookieValue(string cookieName, string value, bool httpOnly, bool secure, string sameSiteMode);

    /// <summary>
    /// Sets the value of a cookie with the specified name and expiration.
    /// </summary>
    /// <param name="cookieName">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="httpOnly">Indicates whether the created cookie should be marked as HTTP only.</param>
    /// <param name="secure">Indicates whether the created cookie should be marked as secure.</param>
    /// <param name="sameSiteMode">Indicates the created cookie's same site status.</param>
    /// <param name="expires">Optional expiration date for the cookie. If null, the cookie is a session cookie.</param>
    /// <remarks>
    /// The value provided by <paramref name="sameSiteMode"/> should match the enum values available from
    /// Microsoft.AspNetCore.Http.SameSiteMode.
    /// This hasn't been used as the parameter directly to avoid a dependency on Microsoft.AspNetCore.Http in
    /// the core project.
    /// </remarks>
    void SetCookieValue(string cookieName, string value, bool httpOnly, bool secure, string sameSiteMode, DateTimeOffset? expires);

    /// <summary>
    /// Determines whether a cookie with the specified name exists.
    /// </summary>
    /// <param name="cookieName">The cookie name.</param>
    bool HasCookie(string cookieName);
}
