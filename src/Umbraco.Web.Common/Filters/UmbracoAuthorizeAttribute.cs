using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class UmbracoAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoAuthorizeAttribute() : this(false, false)
        {
        }

        /// <summary>
        /// Constructor with redirect umbraco login behavior
        /// </summary>
        /// <param name="redirectToUmbracoLogin"></param>
        /// <param name="requireApproval"></param>

        public UmbracoAuthorizeAttribute(bool redirectToUmbracoLogin, bool requireApproval) : base(typeof(UmbracoAuthorizeFilter))
        {
            Arguments = new object[] { redirectToUmbracoLogin, requireApproval };
        }

        /// <summary>
        /// Constructor with redirect url behavior
        /// </summary>
        /// <param name="redirectUrl"></param>
        public UmbracoAuthorizeAttribute(string redirectUrl) : base(typeof(UmbracoAuthorizeFilter))
        {
            Arguments = new object[] { redirectUrl };
        }
    }
}
