using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Security;
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

            private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
            private readonly string[] _appNames;

            /// <summary>
            /// Constructor to set any number of applications that the user needs access to be authorized
            /// </summary>
            /// <param name="backofficeSecurityAccessor"></param>
            /// <param name="appName">
            /// If the user has access to any of the specified apps, they will be authorized.
            /// </param>
            public UmbracoApplicationAuthorizeFilter(IBackOfficeSecurityAccessor backofficeSecurityAccessor, params string[] appName)
            {
                _backofficeSecurityAccessor = backofficeSecurityAccessor;
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

                var authorized = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser != null
                                 && _appNames.Any(app => _backofficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess(
                                     app, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser));

                return authorized;
            }
        }
    }

}
