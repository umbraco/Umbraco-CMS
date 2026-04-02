// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="ClaimsPrincipal" /> and related types.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    ///     Determines whether the specified claims identity is a back office authentication type.
    /// </summary>
    /// <param name="claimsIdentity">The claims identity to check.</param>
    /// <returns><c>true</c> if the identity is authenticated with the back office authentication type; otherwise, <c>false</c>.</returns>
    public static bool IsBackOfficeAuthenticationType(this ClaimsIdentity? claimsIdentity)
    {
        if (claimsIdentity is null)
        {
            return false;
        }

        return claimsIdentity.IsAuthenticated &&
               claimsIdentity.AuthenticationType == Constants.Security.BackOfficeAuthenticationType;
    }

    /// <summary>
    ///     This will return the current back office identity if the IPrincipal is the correct type and authenticated.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static ClaimsIdentity? GetUmbracoIdentity(this IPrincipal principal)
    {
        // If it's already a UmbracoBackOfficeIdentity
        if (principal.Identity is ClaimsIdentity claimsIdentity
            && claimsIdentity.IsBackOfficeAuthenticationType()
            && claimsIdentity.VerifyBackOfficeIdentity(out ClaimsIdentity? backOfficeIdentity))
        {
            return backOfficeIdentity;
        }

        // Check if there's more than one identity assigned and see if it's a UmbracoBackOfficeIdentity and use that
        // We can have assigned more identities if it is a preview request.
        if (principal is ClaimsPrincipal claimsPrincipal)
        {
            ClaimsIdentity? identity =
                claimsPrincipal.Identities.FirstOrDefault(x => x.IsBackOfficeAuthenticationType());
            if (identity is not null)
            {
                claimsIdentity = identity;
                if (claimsIdentity.VerifyBackOfficeIdentity(out backOfficeIdentity))
                {
                    return backOfficeIdentity;
                }
            }
        }

        // Otherwise convert to a UmbracoBackOfficeIdentity if it's auth'd
        if (principal.Identity is ClaimsIdentity claimsIdentity2
            && claimsIdentity2.VerifyBackOfficeIdentity(out backOfficeIdentity))
        {
            return backOfficeIdentity;
        }

        return null;
    }

    /// <summary>
    ///     Returns the remaining seconds on an auth ticket for the user based on the claim applied to the user durnig
    ///     authentication
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static double GetRemainingAuthSeconds(this IPrincipal user) =>
        user.GetRemainingAuthSeconds(DateTimeOffset.UtcNow);

    /// <summary>
    ///     Returns the remaining seconds on an auth ticket for the user based on the claim applied to the user durnig
    ///     authentication
    /// </summary>
    /// <param name="user"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public static double GetRemainingAuthSeconds(this IPrincipal user, DateTimeOffset now)
    {
        if (user is not ClaimsPrincipal claimsPrincipal)
        {
            return 0;
        }

        var ticketExpires = claimsPrincipal.FindFirst(Constants.Security.TicketExpiresClaimType)?.Value;
        if (ticketExpires.IsNullOrWhiteSpace())
        {
            return 0;
        }

        var utcExpired = DateTimeOffset.Parse(ticketExpires!, null, DateTimeStyles.RoundtripKind);

        var secondsRemaining = utcExpired.Subtract(now).TotalSeconds;
        return secondsRemaining;
    }
}
