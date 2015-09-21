using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.Identity;
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
    public class UmbracoBackOfficeIdentity : FormsIdentity
    {
        public static UmbracoBackOfficeIdentity FromClaimsIdentity(ClaimsIdentity identity)
        {
            foreach (var t in RequiredBackOfficeIdentityClaimTypes)
            {
                //if the identity doesn't have the claim, or the claim value is null
                if (identity.HasClaim(x => x.Type == t) == false || identity.HasClaim(x => x.Type == t && x.Value.IsNullOrWhiteSpace()))
                {
                    throw new InvalidOperationException("Cannot create a " + typeof(UmbracoBackOfficeIdentity) + " from " + typeof(ClaimsIdentity) + " since the required claim " + t + " is missing");
                }
            }

            var username = identity.GetUserName();
            var session = identity.FindFirstValue(Constants.Security.SessionIdClaimType);
            var startContentId = identity.FindFirstValue(Constants.Security.StartContentNodeIdClaimType);            
            var startMediaId = identity.FindFirstValue(Constants.Security.StartMediaNodeIdClaimType);

            var culture = identity.FindFirstValue(ClaimTypes.Locality);
            var id = identity.FindFirstValue(ClaimTypes.NameIdentifier);            
            var realName = identity.FindFirstValue(ClaimTypes.GivenName);

            if (username == null || startContentId == null || startMediaId == null 
                || culture == null || id == null 
                || realName == null || session == null)
                throw new InvalidOperationException("Cannot create a " + typeof(UmbracoBackOfficeIdentity) + " from " + typeof(ClaimsIdentity) + " since there are missing required claims");

            int startContentIdAsInt;
            int startMediaIdAsInt;
            if (int.TryParse(startContentId, out startContentIdAsInt) == false || int.TryParse(startMediaId, out startMediaIdAsInt) == false)
            {
                throw new InvalidOperationException("Cannot create a " + typeof(UmbracoBackOfficeIdentity) + " from " + typeof(ClaimsIdentity) + " since the data is not formatted correctly");
            }

            var roles = identity.FindAll(x => x.Type == DefaultRoleClaimType).Select(role => role.Value).ToList();
            var allowedApps = identity.FindAll(x => x.Type == Constants.Security.AllowedApplicationsClaimType).Select(app => app.Value).ToList();

            var userData = new UserData(session)
            {
                SessionId = session,
                AllowedApplications = allowedApps.ToArray(),
                Culture = culture,
                Id = id,
                Roles = roles.ToArray(),
                Username = username,
                RealName = realName,
                StartContentNode = startContentIdAsInt,
                StartMediaNode = startMediaIdAsInt
            };

            return new UmbracoBackOfficeIdentity(identity, userData);
        }

        /// <summary>
        /// Create a back office identity based on user data
        /// </summary>
        /// <param name="userdata"></param>
        public UmbracoBackOfficeIdentity(UserData userdata)
            //This just creates a temp/fake ticket
            : base(new FormsAuthenticationTicket(userdata.Username, true, 10))
        {
            if (userdata == null) throw new ArgumentNullException("userdata");
            UserData = userdata;
            AddUserDataClaims();
        }

        /// <summary>
        /// Create a back office identity based on an existing claims identity
        /// </summary>
        /// <param name="claimsIdentity"></param>
        /// <param name="userdata"></param>
        public UmbracoBackOfficeIdentity(ClaimsIdentity claimsIdentity, UserData userdata)
            //This just creates a temp/fake ticket
            : base(new FormsAuthenticationTicket(userdata.Username, true, 10))
        {
            if (claimsIdentity == null) throw new ArgumentNullException("claimsIdentity");
            if (userdata == null) throw new ArgumentNullException("userdata");

            if (claimsIdentity is FormsIdentity)
            {
                //since it's a forms auth ticket, it is from a cookie so add that claim
                AddClaim(new Claim(ClaimTypes.CookiePath, "/", ClaimValueTypes.String, Issuer, Issuer, this));
            }

            _currentIssuer = claimsIdentity.AuthenticationType;
            UserData = userdata;
            AddExistingClaims(claimsIdentity);
            Actor = claimsIdentity;
            AddUserDataClaims();
        }

        /// <summary>
        /// Create a new identity from a forms auth ticket
        /// </summary>
        /// <param name="ticket"></param>
        public UmbracoBackOfficeIdentity(FormsAuthenticationTicket ticket)
            : base(ticket)
        {
            //since it's a forms auth ticket, it is from a cookie so add that claim
            AddClaim(new Claim(ClaimTypes.CookiePath, "/", ClaimValueTypes.String, Issuer, Issuer, this));

            UserData = JsonConvert.DeserializeObject<UserData>(ticket.UserData);
            AddUserDataClaims();
        }

        /// <summary>
        /// Used for cloning
        /// </summary>
        /// <param name="identity"></param>
        private UmbracoBackOfficeIdentity(UmbracoBackOfficeIdentity identity)
            : base(identity)
        {
            if (identity.Actor != null)
            {
                _currentIssuer = identity.AuthenticationType;
                AddExistingClaims(identity);
                Actor = identity.Clone();
            }

            UserData = identity.UserData;
            AddUserDataClaims();
        }

        public const string Issuer = "UmbracoBackOffice";
        private readonly string _currentIssuer = Issuer;

        /// <summary>
        /// Used during ctor to add existing claims from an existing ClaimsIdentity
        /// </summary>
        /// <param name="claimsIdentity"></param>
        private void AddExistingClaims(ClaimsIdentity claimsIdentity)
        {
            foreach (var claim in claimsIdentity.Claims)
            {
                //In one special case we will replace a claim if it exists already and that is the 
                // Forms auth claim for name which automatically gets added
                TryRemoveClaim(FindFirst(x => x.Type == claim.Type && x.Issuer == "Forms"));

                AddClaim(claim);
            }
        }

        /// <summary>
        /// Returns the required claim types for a back office identity
        /// </summary>
        /// <remarks>
        /// This does not incude the role claim type or allowed apps type since that is a collection and in theory could be empty
        /// </remarks>
        public static IEnumerable<string> RequiredBackOfficeIdentityClaimTypes
        {
            get
            {
                return new[]
                {
                    ClaimTypes.NameIdentifier, //id
                    ClaimTypes.Name,  //username
                    ClaimTypes.GivenName, 
                    Constants.Security.StartContentNodeIdClaimType,
                    Constants.Security.StartMediaNodeIdClaimType, 
                    ClaimTypes.Locality, 
                    Constants.Security.SessionIdClaimType
                };
            }
        } 

        /// <summary>
        /// Adds claims based on the UserData data
        /// </summary>
        private void AddUserDataClaims()
        {
            //This is the id that 'identity' uses to check for the user id
            if (HasClaim(x => x.Type == ClaimTypes.NameIdentifier) == false)
                AddClaim(new Claim(ClaimTypes.NameIdentifier, UserData.Id.ToString(), ClaimValueTypes.Integer32, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == ClaimTypes.Name) == false)
                AddClaim(new Claim(ClaimTypes.Name, UserData.Username, ClaimValueTypes.String, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == ClaimTypes.GivenName) == false)
                AddClaim(new Claim(ClaimTypes.GivenName, UserData.RealName, ClaimValueTypes.String, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == Constants.Security.StartContentNodeIdClaimType) == false)
                AddClaim(new Claim(Constants.Security.StartContentNodeIdClaimType, StartContentNode.ToInvariantString(), ClaimValueTypes.Integer32, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == Constants.Security.StartMediaNodeIdClaimType) == false)
                AddClaim(new Claim(Constants.Security.StartMediaNodeIdClaimType, StartMediaNode.ToInvariantString(), ClaimValueTypes.Integer32, Issuer, Issuer, this));

            if (HasClaim(x => x.Type == ClaimTypes.Locality) == false)
                AddClaim(new Claim(ClaimTypes.Locality, Culture, ClaimValueTypes.String, Issuer, Issuer, this));
            
            ////TODO: Not sure why this is null sometimes, it shouldn't be. Somewhere it's not being set 
            /// I think it's due to some bug I had in chrome, we'll see 
            //if (UserData.SessionId.IsNullOrWhiteSpace())
            //{
            //    UserData.SessionId = Guid.NewGuid().ToString();
            //}

            if (HasClaim(x => x.Type == Constants.Security.SessionIdClaimType) == false)
                AddClaim(new Claim(Constants.Security.SessionIdClaimType, SessionId, ClaimValueTypes.String, Issuer, Issuer, this));

            //Add each app as a separate claim
            if (HasClaim(x => x.Type == Constants.Security.AllowedApplicationsClaimType) == false)
            {
                foreach (var application in AllowedApplications)
                {
                    AddClaim(new Claim(Constants.Security.AllowedApplicationsClaimType, application, ClaimValueTypes.String, Issuer, Issuer, this));    
                }
            }

            //Claims are added by the ClaimsIdentityFactory because our UserStore supports roles, however this identity might
            // not be made with that factory if it was created with a FormsAuthentication ticket so perform the check
            if (HasClaim(x => x.Type == DefaultRoleClaimType) == false)
            {
                //manually add them based on the UserData
                foreach (var roleName in UserData.Roles)
                {
                    AddClaim(new Claim(RoleClaimType, roleName, ClaimValueTypes.String, Issuer, Issuer, this));
                }
            }

            
            
        }

        protected internal UserData UserData { get; private set; }

        /// <summary>
        /// Gets the type of authenticated identity.
        /// </summary>
        /// <returns>
        /// The type of authenticated identity. This property always returns "UmbracoBackOffice".
        /// </returns>
        public override string AuthenticationType
        {
            get { return _currentIssuer; }
        }

        public int StartContentNode
        {
            get { return UserData.StartContentNode; }
        }

        public int StartMediaNode
        {
            get { return UserData.StartMediaNode; }
        }

        public string[] AllowedApplications
        {
            get { return UserData.AllowedApplications; }
        }

        public object Id
        {
            get { return UserData.Id; }
        }

        public string RealName
        {
            get { return UserData.RealName; }
        }

        public string Username
        {
            get { return UserData.Username; }
        }

        public string Culture
        {
            get { return UserData.Culture; }
        }

        public string SessionId
        {
            get { return UserData.SessionId; }
        }

        public string[] Roles
        {
            get { return UserData.Roles; }
        }

        /// <summary>
        /// Gets a copy of the current <see cref="T:UmbracoBackOfficeIdentity"/> instance.
        /// </summary>
        /// <returns>
        /// A copy of the current <see cref="T:UmbracoBackOfficeIdentity"/> instance.
        /// </returns>
        public override ClaimsIdentity Clone()
        {
            return new UmbracoBackOfficeIdentity(this);
        }

    }
}