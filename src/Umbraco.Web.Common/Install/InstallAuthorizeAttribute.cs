using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.Common.Install
{
    public class InstallAuthorizeAttribute : TypeFilterAttribute
    {
        public InstallAuthorizeAttribute() : base(typeof(InstallAuthorizeFilter))
        {
        }

        private class InstallAuthorizeFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var sp = context.HttpContext.RequestServices;
                var runtimeState = sp.GetRequiredService<IRuntimeState>();
                var umbracoContextAccessor = sp.GetRequiredService<IUmbracoContextAccessor>();
                var globalSettings = sp.GetRequiredService<IGlobalSettings>();
                var hostingEnvironment = sp.GetRequiredService<IHostingEnvironment>();

                if (!IsAllowed(runtimeState, umbracoContextAccessor))
                {
                    context.Result = new RedirectResult(globalSettings.GetBackOfficePath(hostingEnvironment));
                }
            }

            private bool IsAllowed(IRuntimeState runtimeState, IUmbracoContextAccessor umbracoContextAccessor)
            {
                try
                {
                    // if not configured (install or upgrade) then we can continue
                    // otherwise we need to ensure that a user is logged in
                    return runtimeState.Level == RuntimeLevel.Install
                           || runtimeState.Level == RuntimeLevel.Upgrade
                           || umbracoContextAccessor.UmbracoContext.Security.ValidateCurrentUser();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
