using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;

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

            private readonly IUmbracoContextAccessor _umbracoContextAccessor;
            private readonly string[] _appNames;

            /// <summary>
            /// Constructor to set any number of applications that the user needs access to be authorized
            /// </summary>
            /// <param name="appName">
            /// If the user has access to any of the specified apps, they will be authorized.
            /// </param>
            public UmbracoApplicationAuthorizeFilter(IUmbracoContextAccessor umbracoContextAccessor, params string[] appName)
            {
                _umbracoContextAccessor = umbracoContextAccessor;
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

                var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
                var authorized = umbracoContext.Security.CurrentUser != null
                                 && _appNames.Any(app => umbracoContext.Security.UserHasSectionAccess(
                                     app, umbracoContext.Security.CurrentUser));

                return authorized;
            }
        }
    }

}
