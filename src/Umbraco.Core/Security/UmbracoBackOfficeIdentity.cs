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
            UserData = userdata;
            AddClaims();
        }

        /// <summary>
        /// Create a new identity from a forms auth ticket
        /// </summary>
        /// <param name="ticket"></param>
        public UmbracoBackOfficeIdentity(FormsAuthenticationTicket ticket)
            : base(ticket)
        {
            UserData = JsonConvert.DeserializeObject<UserData>(ticket.UserData);
            AddClaims();
        }

        /// <summary>
        /// Used for cloning
        /// </summary>
        /// <param name="identity"></param>
        private UmbracoBackOfficeIdentity(UmbracoBackOfficeIdentity identity)
            : base(identity)
        {
            UserData = identity.UserData;
            AddClaims();
        }

        public static string Issuer = "UmbracoBackOffice";

        //TODO: Another option is to create a ClaimsIdentityFactory when everything is wired up... optional though i think
        private void AddClaims()
        {
            AddClaims(new[]
            {
                new Claim(Constants.Security.StartContentNodeIdClaimType, StartContentNode.ToInvariantString(), null, Issuer, Issuer, this),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, StartMediaNode.ToInvariantString(), null, Issuer, Issuer, this),
                new Claim(Constants.Security.AllowedApplicationsClaimType, string.Join(",", AllowedApplications), null, Issuer, Issuer, this),
                new Claim(Constants.Security.UserIdClaimType, Id.ToString(), null, Issuer, Issuer, this),
                new Claim(Constants.Security.CultureClaimType, Culture, null, Issuer, Issuer, this),
                new Claim(Constants.Security.SessionIdClaimType, SessionId, null, Issuer, Issuer, this),
                new Claim(ClaimTypes.Role, string.Join(",", Roles), null, Issuer, Issuer, this)
            });
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
            get { return Issuer; }
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