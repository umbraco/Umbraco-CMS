using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{

    /// <summary>
    /// A custom user identity for the Umbraco backoffice
    /// </summary>
    /// <remarks>
    /// This inherits from FormsIdentity for backwards compatibility reasons since we still support the forms auth cookie, in v8 we can
    /// change over to 'pure' asp.net identity and just inherit from ClaimsIdentity.
    /// </remarks>
    [Serializable]
    public class UmbracoBackOfficeIdentity : ClaimsIdentity
    {
        public static UmbracoBackOfficeIdentity FromClaimsIdentity(ClaimsIdentity identity)
        {
            return new UmbracoBackOfficeIdentity(identity);
        }

        /// <summary>
        /// Creates a new UmbracoBackOfficeIdentity
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        /// <param name="realName"></param>
        /// <param name="startContentNodes"></param>
        /// <param name="startMediaNodes"></param>
        /// <param name="culture"></param>
        /// <param name="sessionId"></param>
        /// <param name="securityStamp"></param>
        /// <param name="allowedApps"></param>
        /// <param name="roles"></param>
        public UmbracoBackOfficeIdentity(int userId, string username, string realName,
            IEnumerable<int> startContentNodes, IEnumerable<int> startMediaNodes, string culture,
            string sessionId, string securityStamp, IEnumerable<string> allowedApps, IEnumerable<string> roles)
            : base(Enumerable.Empty<Claim>(), Constants.Security.BackOfficeAuthenticationType) //this ctor is used to ensure the IsAuthenticated property is true
        {
            if (allowedApps == null) throw new ArgumentNullException(nameof(allowedApps));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            if (string.IsNullOrWhiteSpace(realName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(realName));
            if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(culture));
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(securityStamp)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(securityStamp));
            AddRequiredClaims(userId, username, realName, startContentNodes, startMediaNodes, culture, sessionId, securityStamp, allowedApps, roles);
        }

        /// <summary>
        /// Creates a new UmbracoBackOfficeIdentity
        /// </summary>
        /// <param name="childIdentity">
        /// The original identity created by the ClaimsIdentityFactory
        /// </param>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        /// <param name="realName"></param>
        /// <param name="startContentNodes"></param>
        /// <param name="startMediaNodes"></param>
        /// <param name="culture"></param>
        /// <param name="sessionId"></param>
        /// <param name="securityStamp"></param>
        /// <param name="allowedApps"></param>
        /// <param name="roles"></param>
        public UmbracoBackOfficeIdentity(ClaimsIdentity childIdentity,
            int userId, string username, string realName,
            IEnumerable<int> startContentNodes, IEnumerable<int> startMediaNodes, string culture,
            string sessionId, string securityStamp, IEnumerable<string> allowedApps, IEnumerable<string> roles)
        : base(childIdentity.Claims, Constants.Security.BackOfficeAuthenticationType)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            if (string.IsNullOrWhiteSpace(realName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(realName));
            if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(culture));
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(securityStamp)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(securityStamp));
            Actor = childIdentity;
            AddRequiredClaims(userId, username, realName, startContentNodes, startMediaNodes, culture, sessionId, securityStamp, allowedApps, roles);
        }

        /// <summary>
        /// Create a back office identity based on an existing claims identity
        /// </summary>
        /// <param name="identity"></param>
        private UmbracoBackOfficeIdentity(ClaimsIdentity identity)
            : base(identity.Claims, Constants.Security.BackOfficeAuthenticationType)
        {
            Actor = identity;

            //validate that all claims exist
            foreach (var t in RequiredBackOfficeIdentityClaimTypes)
            {
                //if the identity doesn't have the claim, or the claim value is null
                if (identity.HasClaim(x => x.Type == t) == false || identity.HasClaim(x => x.Type == t && x.Value.IsNullOrWhiteSpace()))
                {
                    throw new InvalidOperationException("Cannot create a " + typeof(UmbracoBackOfficeIdentity) + " from " + typeof(ClaimsIdentity) + " since the required claim " + t + " is missing");
                }
            }
        }

        public const string Issuer = Constants.Security.BackOfficeAuthenticationType;

        /// <summary>
        /// Returns the required claim types for a back office identity
        /// </summary>
        /// <remarks>
        /// This does not incude the role claim type or allowed apps type since that is a collection and in theory could be empty
        /// </remarks>
        public static IEnumerable<string> RequiredBackOfficeIdentityClaimTypes => new[]
        {
            ClaimTypes.NameIdentifier, //id
            ClaimTypes.Name,  //username
            ClaimTypes.GivenName,
            Constants.Security.StartContentNodeIdClaimType,
            Constants.Security.StartMediaNodeIdClaimType,
            ClaimTypes.Locality,
            Constants.Security.SessionIdClaimType,
            Microsoft.AspNet.Identity.Constants.DefaultSecurityStampClaimType
        };

        /// <summary>
        /// Adds claims based on the ctor data
        /// </summary>
        private void AddRequiredClaims(int userId, string username, string realName,
            IEnumerable<int> startContentNodes, IEnumerable<int> startMediaNodes, string culture,
            string sessionId, string securityStamp, IEnumerable<string> allowedApps, IEnumerable<string> roles)
        {
            //This is the id that 'identity' uses to check for the user id
            if (HasClaim(x => x.Type == ClaimTypes.NameIdentifier) == false)
                AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToInvariantString(), ClaimValueTypes.Integer32, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == ClaimTypes.Name) == false)
                AddClaim(new Claim(ClaimTypes.Name, username, ClaimValueTypes.String, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == ClaimTypes.GivenName) == false)
                AddClaim(new Claim(ClaimTypes.GivenName, realName, ClaimValueTypes.String, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == Constants.Security.StartContentNodeIdClaimType) == false && startContentNodes != null)
            {
                foreach (var startContentNode in startContentNodes)
                {
                    AddClaim(new Claim(Constants.Security.StartContentNodeIdClaimType, startContentNode.ToInvariantString(), ClaimValueTypes.Integer32, Issuer, Issuer, this));    
                }
            }

            if (HasClaim(x => x.Type == Constants.Security.StartMediaNodeIdClaimType) == false && startMediaNodes != null)
            {
                foreach (var startMediaNode in startMediaNodes)
                {
                    AddClaim(new Claim(Constants.Security.StartMediaNodeIdClaimType, startMediaNode.ToInvariantString(), ClaimValueTypes.Integer32, Issuer, Issuer, this));
                }
            }

            if (HasClaim(x => x.Type == ClaimTypes.Locality) == false)
                AddClaim(new Claim(ClaimTypes.Locality, culture, ClaimValueTypes.String, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == Constants.Security.SessionIdClaimType) == false && SessionId.IsNullOrWhiteSpace() == false)
                AddClaim(new Claim(Constants.Security.SessionIdClaimType, sessionId, ClaimValueTypes.String, Issuer, Issuer, this));

            //The security stamp claim is also required... this is because this claim type is hard coded
            // by the SecurityStampValidator, see: https://katanaproject.codeplex.com/workitem/444
            if (HasClaim(x => x.Type == Microsoft.AspNet.Identity.Constants.DefaultSecurityStampClaimType) == false)
                AddClaim(new Claim(Microsoft.AspNet.Identity.Constants.DefaultSecurityStampClaimType, securityStamp, ClaimValueTypes.String, Issuer, Issuer, this));

            //Add each app as a separate claim
            if (HasClaim(x => x.Type == Constants.Security.AllowedApplicationsClaimType) == false && allowedApps != null)
            {
                foreach (var application in allowedApps)
                {
                    AddClaim(new Claim(Constants.Security.AllowedApplicationsClaimType, application, ClaimValueTypes.String, Issuer, Issuer, this));
                }
            }

            //Claims are added by the ClaimsIdentityFactory because our UserStore supports roles, however this identity might
            // not be made with that factory if it was created with a different ticket so perform the check
            if (HasClaim(x => x.Type == DefaultRoleClaimType) == false && roles != null)
            {
                //manually add them
                foreach (var roleName in roles)
                {
                    AddClaim(new Claim(RoleClaimType, roleName, ClaimValueTypes.String, Issuer, Issuer, this));
                }
            }

        }
        
        /// <inheritdoc />
        /// <summary>
        /// Gets the type of authenticated identity.
        /// </summary>
        /// <returns>
        /// The type of authenticated identity. This property always returns "UmbracoBackOffice".
        /// </returns>
        public override string AuthenticationType => Issuer;

        private int[] _startContentNodes;   
        public int[] StartContentNodes => _startContentNodes ?? (_startContentNodes = FindAll(x => x.Type == Constants.Security.StartContentNodeIdClaimType).Select(app => int.TryParse(app.Value, out var i) ? i : default).Where(x => x != default).ToArray());

        private int[] _startMediaNodes;
        public int[] StartMediaNodes => _startMediaNodes ?? (_startMediaNodes = FindAll(x => x.Type == Constants.Security.StartMediaNodeIdClaimType).Select(app => int.TryParse(app.Value, out var i) ? i : default).Where(x => x != default).ToArray());

        private string[] _allowedApplications;
        public string[] AllowedApplications => _allowedApplications ?? (_allowedApplications = FindAll(x => x.Type == Constants.Security.AllowedApplicationsClaimType).Select(app => app.Value).ToArray());

        public int Id => int.Parse(this.FindFirstValue(ClaimTypes.NameIdentifier));

        public string RealName => this.FindFirstValue(ClaimTypes.GivenName);

        public string Username => this.GetUserName();

        public string Culture => this.FindFirstValue(ClaimTypes.Locality);

        public string SessionId
        {
            get => this.FindFirstValue(Constants.Security.SessionIdClaimType);
            set
            {
                var existing = FindFirst(Constants.Security.SessionIdClaimType);
                if (existing != null)
                    TryRemoveClaim(existing);
                AddClaim(new Claim(Constants.Security.SessionIdClaimType, value, ClaimValueTypes.String, Issuer, Issuer, this));
            }
        }

        public string SecurityStamp => this.FindFirstValue(Microsoft.AspNet.Identity.Constants.DefaultSecurityStampClaimType);

        public string[] Roles => this.FindAll(x => x.Type == DefaultRoleClaimType).Select(role => role.Value).ToArray();
        
    }
}
