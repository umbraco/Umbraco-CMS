// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="IIdentity" />.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Ensures that the thread culture is set based on the back office user's culture.
    /// </summary>
    /// <param name="identity">The identity.</param>
    public static void EnsureCulture(this IIdentity identity)
    {
        CultureInfo? culture = GetCulture(identity);
        if (culture is not null)
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    /// <summary>
    /// Gets the culture string from the back office user.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <returns>
    /// The culture string.
    /// </returns>
    public static string? GetCultureString(this IIdentity identity)
    {
        if (identity is ClaimsIdentity umbIdentity &&
            umbIdentity.VerifyBackOfficeIdentity(out _) &&
            umbIdentity.IsAuthenticated)
        {
            return umbIdentity.GetCultureString();
        }

        return null;
    }

    /// <summary>
    /// Gets the culture from the back office user.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <returns>
    /// The culture.
    /// </returns>
    public static CultureInfo? GetCulture(this IIdentity identity)
    {
        string? culture = identity.GetCultureString();
        if (!string.IsNullOrEmpty(culture))
        {
            return CultureInfo.GetCultureInfo(culture);
        }

        return null;
    }
}
