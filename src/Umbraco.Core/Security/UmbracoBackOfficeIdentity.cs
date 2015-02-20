using System;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;

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

            _currentIssuer = claimsIdentity.AuthenticationType;
            UserData = userdata;
            AddClaims(claimsIdentity);
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
                AddClaims(identity);
                Actor = identity.Clone();
            }

            UserData = identity.UserData;
            AddUserDataClaims();
        }

        public const string Issuer = "UmbracoBackOffice";
        private readonly string _currentIssuer = Issuer;

        private void AddClaims(ClaimsIdentity claimsIdentity)
        {
            foreach (var claim in claimsIdentity.Claims)
            {
                AddClaim(claim);
            }
        }

        /// <summary>
        /// Adds claims based on the UserData data
        /// </summary>
        private void AddUserDataClaims()
        {
            AddClaims(new[]
            {
                //This is the id that 'identity' uses to check for the user id
                new Claim(ClaimTypes.NameIdentifier, Id.ToString(), null, Issuer, Issuer, this), 

                new Claim(Constants.Security.StartContentNodeIdClaimType, StartContentNode.ToInvariantString(), null, Issuer, Issuer, this),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, StartMediaNode.ToInvariantString(), null, Issuer, Issuer, this),
                new Claim(Constants.Security.AllowedApplicationsClaimType, string.Join(",", AllowedApplications), null, Issuer, Issuer, this),
                
                //TODO: Similar one created by the ClaimsIdentityFactory<TUser, TKey> not sure we need this
                new Claim(Constants.Security.CultureClaimType, Culture, null, Issuer, Issuer, this)                

                //TODO: Role claims are added by the default ClaimsIdentityFactory<TUser, TKey> based on the result from 
                // the user manager manager.GetRolesAsync method so not sure if we can do that there or needs to be done here
                // and each role should be a different claim, not a single string 

                //new Claim(ClaimTypes.Role, string.Join(",", Roles), null, Issuer, Issuer, this)
            });

            //TODO: Find out why sessionid is null - this depends on how the identity is created!
            if (SessionId.IsNullOrWhiteSpace() == false)
            {
                AddClaim(new Claim(Constants.Security.SessionIdClaimType, SessionId, null, Issuer, Issuer, this));    
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