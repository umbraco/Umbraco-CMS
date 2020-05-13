using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class UmbracoAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoAuthorizeAttribute() : base(typeof(UmbracoAuthorizeFilter))
        {
        }

        /// <summary>
        /// Constructor with redirect umbraco login behavior
        /// </summary>
        /// <param name="redirectToUmbracoLogin"></param>
        public UmbracoAuthorizeAttribute(bool redirectToUmbracoLogin) : base(typeof(UmbracoAuthorizeFilter))
        {
            Arguments = new object[] { redirectToUmbracoLogin };
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
