// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static class ClaimsIdentityExtensions
{
    /// <summary>
    ///     Returns the required claim types for a back office identity
    /// </summary>
    /// <remarks>
    ///     This does not include the role claim type or allowed apps type since that is a collection and in theory could be
    ///     empty
    /// </remarks>
    public static IEnumerable<string> RequiredBackOfficeClaimTypes => new[]
    {
        ClaimTypes.NameIdentifier, // id
        ClaimTypes.Name, // username
        ClaimTypes.GivenName,

        // Constants.Security.StartContentNodeIdClaimType, These seem to be able to be null...
        // Constants.Security.StartMediaNodeIdClaimType,
        ClaimTypes.Locality, Constants.Security.SecurityStampClaimType,
    };

    public static T? GetUserId<T>(this IIdentity identity)
    {
        var strId = identity.GetUserId();
        Attempt<T> converted = strId.TryConvertTo<T>();
        return converted.Result ?? default;
    }

    /// <summary>
    ///     Returns the user id from the <see cref="IIdentity" /> of either the claim type
    ///     <see cref="ClaimTypes.NameIdentifier" /> or "sub"
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>
    ///     The string value of the user id if found otherwise null
    /// </returns>
    public static string? GetUserId(this IIdentity identity)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        string? userId = null;
        if (identity is ClaimsIdentity claimsIdentity)
        {
            userId = claimsIdentity.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? claimsIdentity.FindFirstValue("sub");
        }

        return userId;
    }

    /// <summary>
    ///     Returns the user name from the <see cref="IIdentity" /> of either the claim type <see cref="ClaimTypes.Name" /> or
    ///     "preferred_username"
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>
    ///     The string value of the user name if found otherwise null
    /// </returns>
    public static string? GetUserName(this IIdentity identity)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        string? username = null;
        if (identity is ClaimsIdentity claimsIdentity)
        {
            username = claimsIdentity.FindFirstValue(ClaimTypes.Name)
                       ?? claimsIdentity.FindFirstValue("preferred_username");
        }

        return username;
    }

    public static string? GetEmail(this IIdentity identity)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        string? email = null;
        if (identity is ClaimsIdentity claimsIdentity)
        {
            email = claimsIdentity.FindFirstValue(ClaimTypes.Email);
        }

        return email;
    }

    /// <summary>
    ///     Returns the first claim value found in the <see cref="ClaimsIdentity" /> for the given claimType
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="claimType"></param>
    /// <returns>
    ///     The string value of the claim if found otherwise null
    /// </returns>
    public static string? FindFirstValue(this ClaimsIdentity identity, string claimType)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        return identity.FindFirst(claimType)?.Value;
    }

    /// <summary>
    ///     Verify that a ClaimsIdentity has all the required claim types
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="verifiedIdentity">Verified identity wrapped in a ClaimsIdentity with BackOfficeAuthentication type</param>
    /// <returns>True if ClaimsIdentity</returns>
    public static bool VerifyBackOfficeIdentity(
        this ClaimsIdentity identity,
        [MaybeNullWhen(false)] out ClaimsIdentity verifiedIdentity)
    {
        if (identity is null)
        {
            verifiedIdentity = null;
            return false;
        }

        // Validate that all required claims exist
        foreach (var claimType in RequiredBackOfficeClaimTypes)
        {
            if (identity.HasClaim(x => x.Type == claimType) == false ||
                identity.HasClaim(x => x.Type == claimType && x.Value.IsNullOrWhiteSpace()))
            {
                verifiedIdentity = null;
                return false;
            }
        }

        verifiedIdentity = identity.AuthenticationType == Constants.Security.BackOfficeAuthenticationType
            ? identity
            : new ClaimsIdentity(identity.Claims, Constants.Security.BackOfficeAuthenticationType);
        return true;
    }

    /// <summary>
    ///     Add the required claims to be a BackOffice ClaimsIdentity
    /// </summary>
    /// <param name="identity">this</param>
    /// <param name="userId">The users Id</param>
    /// <param name="username">Username</param>
    /// <param name="realName">Real name</param>
    /// <param name="startContentNodes">Start content nodes</param>
    /// <param name="startMediaNodes">Start media nodes</param>
    /// <param name="culture">The locality of the user</param>
    /// <param name="securityStamp">Security stamp</param>
    /// <param name="allowedApps">Allowed apps</param>
    /// <param name="roles">Roles</param>
    public static void AddRequiredClaims(this ClaimsIdentity identity, string userId, string username, string realName, IEnumerable<int>? startContentNodes, IEnumerable<int>? startMediaNodes, string culture, string securityStamp, IEnumerable<string> allowedApps, IEnumerable<string> roles)
    {
        // This is the id that 'identity' uses to check for the user id
        if (identity.HasClaim(x => x.Type == ClaimTypes.NameIdentifier) == false)
        {
            identity.AddClaim(new Claim(
                ClaimTypes.NameIdentifier,
                userId,
                ClaimValueTypes.String,
                Constants.Security.BackOfficeAuthenticationType,
                Constants.Security.BackOfficeAuthenticationType,
                identity));
        }

        if (identity.HasClaim(x => x.Type == ClaimTypes.Name) == false)
        {
            identity.AddClaim(new Claim(
                ClaimTypes.Name,
                username,
                ClaimValueTypes.String,
                Constants.Security.BackOfficeAuthenticationType,
                Constants.Security.BackOfficeAuthenticationType,
                identity));
        }

        if (identity.HasClaim(x => x.Type == ClaimTypes.GivenName) == false)
        {
            identity.AddClaim(new Claim(
                ClaimTypes.GivenName,
                realName,
                ClaimValueTypes.String,
                Constants.Security.BackOfficeAuthenticationType,
                Constants.Security.BackOfficeAuthenticationType,
                identity));
        }

        if (identity.HasClaim(x => x.Type == Constants.Security.StartContentNodeIdClaimType) == false &&
            startContentNodes != null)
        {
            foreach (var startContentNode in startContentNodes)
            {
                identity.AddClaim(new Claim(
                    Constants.Security.StartContentNodeIdClaimType,
                    startContentNode.ToInvariantString(),
                    ClaimValueTypes.Integer32,
                    Constants.Security.BackOfficeAuthenticationType,
                    Constants.Security.BackOfficeAuthenticationType,
                    identity));
            }
        }

        if (identity.HasClaim(x => x.Type == Constants.Security.StartMediaNodeIdClaimType) == false &&
            startMediaNodes != null)
        {
            foreach (var startMediaNode in startMediaNodes)
            {
                identity.AddClaim(new Claim(
                    Constants.Security.StartMediaNodeIdClaimType,
                    startMediaNode.ToInvariantString(),
                    ClaimValueTypes.Integer32,
                    Constants.Security.BackOfficeAuthenticationType,
                    Constants.Security.BackOfficeAuthenticationType,
                    identity));
            }
        }

        if (identity.HasClaim(x => x.Type == ClaimTypes.Locality) == false)
        {
            identity.AddClaim(new Claim(
                ClaimTypes.Locality,
                culture,
                ClaimValueTypes.String,
                Constants.Security.BackOfficeAuthenticationType,
                Constants.Security.BackOfficeAuthenticationType,
                identity));
        }

        // The security stamp claim is also required
        if (identity.HasClaim(x => x.Type == Constants.Security.SecurityStampClaimType) == false)
        {
            identity.AddClaim(new Claim(
                Constants.Security.SecurityStampClaimType,
                securityStamp,
                ClaimValueTypes.String,
                Constants.Security.BackOfficeAuthenticationType,
                Constants.Security.BackOfficeAuthenticationType,
                identity));
        }

        // Add each app as a separate claim
        if (identity.HasClaim(x => x.Type == Constants.Security.AllowedApplicationsClaimType) == false && allowedApps != null)
        {
            foreach (var application in allowedApps)
            {
                identity.AddClaim(new Claim(
                    Constants.Security.AllowedApplicationsClaimType,
                    application,
                    ClaimValueTypes.String,
                    Constants.Security.BackOfficeAuthenticationType,
                    Constants.Security.BackOfficeAuthenticationType,
                    identity));
            }
        }

        // Claims are added by the ClaimsIdentityFactory because our UserStore supports roles, however this identity might
        // not be made with that factory if it was created with a different ticket so perform the check
        if (identity.HasClaim(x => x.Type == ClaimsIdentity.DefaultRoleClaimType) == false && roles != null)
        {
            // Manually add them
            foreach (var roleName in roles)
            {
                identity.AddClaim(new Claim(
                    identity.RoleClaimType,
                    roleName,
                    ClaimValueTypes.String,
                    Constants.Security.BackOfficeAuthenticationType,
                    Constants.Security.BackOfficeAuthenticationType,
                    identity));
            }
        }
    }

    /// <summary>
    ///     Get the start content nodes from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Array of start content nodes</returns>
    public static int[] GetStartContentNodes(this ClaimsIdentity identity) =>
        identity.FindAll(x => x.Type == Constants.Security.StartContentNodeIdClaimType)
            .Select(node => int.TryParse(node.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                ? i
                : default)
            .Where(x => x != default).ToArray();

    /// <summary>
    ///     Get the start media nodes from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Array of start media nodes</returns>
    public static int[] GetStartMediaNodes(this ClaimsIdentity identity) =>
        identity.FindAll(x => x.Type == Constants.Security.StartMediaNodeIdClaimType)
            .Select(node => int.TryParse(node.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                ? i
                : default)
            .Where(x => x != default).ToArray();

    /// <summary>
    ///     Get the allowed applications from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns></returns>
    public static string[] GetAllowedApplications(this ClaimsIdentity identity) => identity
        .FindAll(x => x.Type == Constants.Security.AllowedApplicationsClaimType).Select(app => app.Value).ToArray();

    /// <summary>
    ///     Get the user ID from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>User ID as integer</returns>
    public static int? GetId(this ClaimsIdentity identity)
    {
        var firstValue = identity.FindFirstValue(ClaimTypes.NameIdentifier);
        if (firstValue is not null)
        {
            return int.Parse(firstValue, CultureInfo.InvariantCulture);
        }

        return null;
    }

    /// <summary>
    ///     Get the real name belonging to the user from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Real name of the user</returns>
    public static string? GetRealName(this ClaimsIdentity identity) => identity.FindFirstValue(ClaimTypes.GivenName);

    /// <summary>
    ///     Get the username of the user from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Username of the user</returns>
    public static string? GetUsername(this ClaimsIdentity identity) => identity.FindFirstValue(ClaimTypes.Name);

    /// <summary>
    ///     Get the culture string from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Culture string</returns>
    public static string? GetCultureString(this ClaimsIdentity identity) =>
        identity.FindFirstValue(ClaimTypes.Locality);

    /// <summary>
    ///     Get the security stamp from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Security stamp</returns>
    public static string? GetSecurityStamp(this ClaimsIdentity identity) =>
        identity.FindFirstValue(Constants.Security.SecurityStampClaimType);

    /// <summary>
    ///     Get the roles assigned to a user from a ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Array of roles</returns>
    public static string[] GetRoles(this ClaimsIdentity identity) => identity
        .FindAll(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Select(role => role.Value).ToArray();

    /// <summary>
    ///     Adds or updates and existing claim.
    /// </summary>
    public static void AddOrUpdateClaim(this ClaimsIdentity identity, Claim? claim)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        if (claim is not null)
        {
            Claim? existingClaim = identity.Claims.FirstOrDefault(x => x.Type == claim.Type);
            identity.TryRemoveClaim(existingClaim);

            identity.AddClaim(claim);
        }
    }
}
