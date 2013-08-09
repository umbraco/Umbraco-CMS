using System.Web.Http;
using System.Web.Http.Controllers;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the current user has access to the specified application
    /// </summary>
    internal sealed class UmbracoApplicationAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly string _appName;

        public UmbracoApplicationAuthorizeAttribute(string appName)
        {
            _appName = appName;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (Enable == false)
            {
                return true;
            }

            return UmbracoContext.Current.Security.CurrentUser != null 
                && UmbracoContext.Current.Security.UserHasAppAccess(_appName, UmbracoContext.Current.Security.CurrentUser);
        }
    }
}