using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Filters
{
    public class UmbracoApplicationAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly string[] _appNames;

        /// <summary>
        /// Constructor to set any number of applications that the user needs access to be authorized
        /// </summary>
        /// <param name="appName">
        /// If the user has access to any of the specified apps, they will be authorized.
        /// </param>
        public UmbracoApplicationAuthorizeAttribute(params string[] appName)
        {
            _appNames = appName;
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var umbracoContextAccessor = context.HttpContext.RequestServices.GetRequiredService<IUmbracoContextAccessor>();
            if (!IsAuthorized(umbracoContextAccessor))
            {
                context.Result = new ForbidResult();
            }
        }

        private bool IsAuthorized(IUmbracoContextAccessor umbracoContextAccessor)
        {
            if (Enable == false)
            {
                return true;
            }

            var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
            var authorized = umbracoContext.Security.CurrentUser != null
                            && _appNames.Any(app => umbracoContext.Security.UserHasSectionAccess(
                                app, umbracoContext.Security.CurrentUser));

            return authorized;
        }
    }
}
