// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace Umbraco.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    ///     Ensures that the thread culture is set based on the back office user's culture
    /// </summary>
    public static void EnsureCulture(this IIdentity identity)
    {
        CultureInfo? culture = GetCulture(identity);
        if (!(culture is null))
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    public static CultureInfo? GetCulture(this IIdentity identity)
    {
        if (identity is ClaimsIdentity umbIdentity && umbIdentity.VerifyBackOfficeIdentity(out _) &&
            umbIdentity.IsAuthenticated && umbIdentity.GetCultureString() is not null)
        {
            return CultureInfo.GetCultureInfo(umbIdentity.GetCultureString()!);
        }

        return null;
    }
}
