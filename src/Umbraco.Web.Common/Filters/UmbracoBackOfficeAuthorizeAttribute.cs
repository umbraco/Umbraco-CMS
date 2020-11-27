using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class UmbracoBackOfficeAuthorizeAttribute : TypeFilterAttribute, IAuthorizeData
    {
        // Implements IAuthorizeData to return the back office scheme so that all requests with this attributes
        // get authenticated with this scheme.
        // TODO: We'll have to refactor this as part of the authz policy changes.
        public string AuthenticationSchemes { get; set; } = Umbraco.Core.Constants.Security.BackOfficeAuthenticationType;
        public string Policy { get; set; }
        public string Roles { get; set; }
    
        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoBackOfficeAuthorizeAttribute() : this(false, false)
        {
        }

        /// <summary>
        /// Constructor with redirect umbraco login behavior
        /// </summary>
        /// <param name="redirectToUmbracoLogin"></param>
        /// <param name="requireApproval"></param>

        public UmbracoBackOfficeAuthorizeAttribute(bool redirectToUmbracoLogin, bool requireApproval) : base(typeof(UmbracoBackOfficeAuthorizeFilter))
        {
            Arguments = new object[] { redirectToUmbracoLogin, requireApproval };
        }

        /// <summary>
        /// Constructor with redirect url behavior
        /// </summary>
        /// <param name="redirectUrl"></param>
        public UmbracoBackOfficeAuthorizeAttribute(string redirectUrl) : base(typeof(UmbracoBackOfficeAuthorizeFilter))
        {
            Arguments = new object[] { redirectUrl };
        }
    }
}
