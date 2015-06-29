using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the current user has access to the specified application
    /// </summary>
    public sealed class UmbracoApplicationAuthorizeAttribute : OverridableAuthorizationAttribute
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly string[] _appNames;

        /// <summary>
        /// Constructor to set any number of applications that the user needs access to to be authorized
        /// </summary>
        /// <param name="appName">
        /// If the user has access to any of the specified apps, they will be authorized.
        /// </param>
        public UmbracoApplicationAuthorizeAttribute(params string[] appName)
        {
            _appNames = appName;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (Enable == false)
            {
                return true;
            }

            return UmbracoContext.Current.Security.CurrentUser != null
                   && _appNames.Any(app => UmbracoContext.Current.Security.UserHasAppAccess(
                       app, UmbracoContext.Current.Security.CurrentUser));
        }
    }
}