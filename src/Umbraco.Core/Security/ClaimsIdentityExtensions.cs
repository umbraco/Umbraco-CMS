// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Umbraco.Core;

namespace Umbraco.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        /// <summary>
        /// Returns the required claim types for a back office identity
        /// </summary>
        /// <remarks>
        /// This does not include the role claim type or allowed apps type since that is a collection and in theory could be empty
        /// </remarks>
        public static IEnumerable<string> RequiredBackOfficeClaimTypes => new[]
        {
            ClaimTypes.NameIdentifier, //id
            ClaimTypes.Name,  //username
            ClaimTypes.GivenName,
            Constants.Security.StartContentNodeIdClaimType,
            Constants.Security.StartMediaNodeIdClaimType,
            ClaimTypes.Locality,
            Constants.Security.SecurityStampClaimType
        };

        public const string Issuer = Constants.Security.BackOfficeAuthenticationType;

        /// <summary>
        /// Verify that a ClaimsIdentity has all the required claim types
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="verifiedIdentity">Verified identity wrapped in a ClaimsIdentity with BackOfficeAuthentication type</param>
        /// <returns>True if ClaimsIdentity</returns>
        public static bool VerifyBackOfficeIdentity(this ClaimsIdentity identity, out ClaimsIdentity verifiedIdentity)
        {
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

            verifiedIdentity = new ClaimsIdentity(identity.Claims, Constants.Security.BackOfficeAuthenticationType);
            return true;
        }

        public static void AddRequiredClaims(this ClaimsIdentity identity, string userId, string username,
            string realName, IEnumerable<int> startContentNodes, IEnumerable<int> startMediaNodes, string culture,
            string securityStamp, IEnumerable<string> allowedApps, IEnumerable<string> roles)
        {
            //This is the id that 'identity' uses to check for the user id
            if (identity.HasClaim(x => x.Type == ClaimTypes.NameIdentifier) == false)
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String,
                    Issuer, Issuer, identity));
            }

            if (identity.HasClaim(x => x.Type == ClaimTypes.Name) == false)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, username, ClaimValueTypes.String, Issuer, Issuer, identity));
            }

            if (identity.HasClaim(x => x.Type == ClaimTypes.GivenName) == false)
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, realName, ClaimValueTypes.String, Issuer, Issuer, identity));
            }

            if (identity.HasClaim(x => x.Type == Constants.Security.StartContentNodeIdClaimType) == false &&
                startContentNodes != null)
            {
                foreach (var startContentNode in startContentNodes)
                {
                    identity.AddClaim(new Claim(Constants.Security.StartContentNodeIdClaimType, startContentNode.ToInvariantString(), ClaimValueTypes.Integer32, Issuer, Issuer, identity));
                }
            }

            if (identity.HasClaim(x => x.Type == Constants.Security.StartMediaNodeIdClaimType) == false)
            {
                foreach (var startMediaNode in startMediaNodes)
                {
                    identity.AddClaim(new Claim(Constants.Security.StartMediaNodeIdClaimType, startMediaNode.ToInvariantString(), ClaimValueTypes.Integer32, Issuer, Issuer, identity));
                }
            }

            if (identity.HasClaim(x => x.Type == ClaimTypes.Locality) == false)
            {
                identity.AddClaim(new Claim(ClaimTypes.Locality, culture, ClaimValueTypes.String, Issuer, Issuer, identity));
            }

            // The security stamp claim is also required
            if (identity.HasClaim(x => x.Type == Constants.Security.SecurityStampClaimType) == false)
            {
                identity.AddClaim(new Claim(Constants.Security.SecurityStampClaimType, securityStamp, ClaimValueTypes.String, Issuer, Issuer, identity));
            }

            // Add each app as a separate claim
            if (identity.HasClaim(x => x.Type == Constants.Security.AllowedApplicationsClaimType) == false &&
                allowedApps != null)
            {
                foreach (var application in allowedApps)
                {
                    identity.AddClaim(new Claim(Constants.Security.AllowedApplicationsClaimType, application, ClaimValueTypes.String, Issuer, Issuer, identity));
                }
            }

            // Claims are added by the ClaimsIdentityFactory because our UserStore supports roles, however this identity might
            // not be made with that factory if it was created with a different ticket so perform the check
            if (identity.HasClaim(x => x.Type == ClaimsIdentity.DefaultRoleClaimType) == false && roles != null)
            {
                // Manually add them
                foreach (var roleName in roles)
                {
                    identity.AddClaim(new Claim(identity.RoleClaimType, roleName, ClaimValueTypes.String, Issuer, Issuer, identity));
                }
            }
        }

        public static int[] GetStartContentNodes(this ClaimsIdentity identity) =>
            identity.FindAll(x => x.Type == Constants.Security.StartContentNodeIdClaimType)
                .Select(node => int.TryParse(node.Value, out var i) ? i : default)
                .Where(x => x != default).ToArray();

        public static int[] GetStartMediaNodes(this ClaimsIdentity identity) =>
            identity.FindAll(x => x.Type == Constants.Security.StartMediaNodeIdClaimType)
                .Select(node => int.TryParse(node.Value, out var i) ? i : default)
                .Where(x => x != default).ToArray();

        public static string[] GetAllowedApplications(this ClaimsIdentity identity) => identity
            .FindAll(x => x.Type == Constants.Security.AllowedApplicationsClaimType).Select(app => app.Value).ToArray();

        public static int GetId(this ClaimsIdentity identity) => int.Parse(identity.FindFirstValue(ClaimTypes.NameIdentifier));

        public static string GetRealName(this ClaimsIdentity identity) => identity.FindFirstValue(ClaimTypes.GivenName);

        public static string GetUsername(this ClaimsIdentity identity) => identity.FindFirstValue(ClaimTypes.Name);

        public static string GetCultureString(this ClaimsIdentity identity) => identity.FindFirstValue(ClaimTypes.Locality);

        public static string GetSecurityStamp(this ClaimsIdentity identity) => identity.FindFirstValue(Constants.Security.SecurityStampClaimType);

        public static string[] GetRoles(this ClaimsIdentity identity) => identity
            .FindAll(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Select(role => role.Value).ToArray();
    }
}
