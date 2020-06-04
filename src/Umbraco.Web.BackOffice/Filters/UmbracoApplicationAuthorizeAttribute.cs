using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    public class UmbracoApplicationAuthorizeAttribute : TypeFilterAttribute
    {
        public UmbracoApplicationAuthorizeAttribute(params string[] appName) : base(typeof(UmbracoApplicationAuthorizeFilter))
        {
            base.Arguments = new object[]
            {
                appName
            };
        }

        private class UmbracoApplicationAuthorizeFilter : IAuthorizationFilter
        {
            /// <summary>
            /// Can be used by unit tests to enable/disable this filter
            /// </summary>
            internal static bool Enable = true;

            private readonly IWebSecurity _webSecurity;
            private readonly string[] _appNames;

            /// <summary>
            /// Constructor to set any number of applications that the user needs access to be authorized
            /// </summary>
            /// <param name="webSecurity"></param>
            /// <param name="appName">
            /// If the user has access to any of the specified apps, they will be authorized.
            /// </param>
            public UmbracoApplicationAuthorizeFilter(IWebSecurity webSecurity, params string[] appName)
            {
                _webSecurity = webSecurity;
                _appNames = appName;
            }


            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!IsAuthorized())
                {
                    context.Result = new ForbidResult();
                }
            }

            private bool IsAuthorized()
            {
                if (Enable == false)
                {
                    return true;
                }

                var authorized = _webSecurity.CurrentUser != null
                                 && _appNames.Any(app => _webSecurity.UserHasSectionAccess(
                                     app, _webSecurity.CurrentUser));

                return authorized;
            }
        }
    }

}
