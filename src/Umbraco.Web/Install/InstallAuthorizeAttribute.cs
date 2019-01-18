using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Install
{
    /// <summary>
    /// Ensures authorization occurs for the installer if it has already completed.
    /// If install has not yet occurred then the authorization is successful
    /// </summary>
    internal class InstallAuthorizeAttribute : AuthorizeAttribute
    {
        // see note in HttpInstallAuthorizeAttribute
        private readonly UmbracoContext _umbracoContext;
        private readonly IRuntimeState _runtimeState;

        private IRuntimeState RuntimeState => _runtimeState ?? Current.RuntimeState;

        private UmbracoContext UmbracoContext => _umbracoContext ?? Current.UmbracoContext;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="runtimeState"></param>
        public InstallAuthorizeAttribute(UmbracoContext umbracoContext, IRuntimeState runtimeState)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            _umbracoContext = umbracoContext;
            _runtimeState = runtimeState;
        }

        public InstallAuthorizeAttribute()
        { }

        /// <summary>
        /// Ensures that the user must be logged in or that the application is not configured just yet.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            try
            {
                // if not configured (install or upgrade) then we can continue
                // otherwise we need to ensure that a user is logged in
                return RuntimeState.Level == RuntimeLevel.Install
                    || RuntimeState.Level == RuntimeLevel.Upgrade
                    || UmbracoContext.Security.ValidateCurrentUser();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Override to redirect instead of throwing an exception
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult(SystemDirectories.Umbraco.EnsureEndsWith('/'));
        }
    }
}
