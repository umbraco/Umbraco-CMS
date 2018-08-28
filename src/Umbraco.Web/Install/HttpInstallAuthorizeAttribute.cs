using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Install
{
    /// <summary>
    /// Ensures authorization occurs for the installer if it has already completed.
    /// If install has not yet occured then the authorization is successful.
    /// </summary>
    internal class HttpInstallAuthorizeAttribute : AuthorizeAttribute
    {
        // todo - cannot inject UmbracoContext nor RuntimeState in the attribute, read:
        // http://stackoverflow.com/questions/30096903/dependency-injection-inside-a-filterattribute-in-asp-net-mvc-6
        //  https://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=98 - don't do it!
        //  http://blog.ploeh.dk/2014/06/13/passive-attributes/ - passive attributes
        //  http://xunitpatterns.com/Humble%20Object.html - humble objects
        //
        // so... either access them via Current service locator, OR use an action filter alongside this attribute (see articles).
        // the second solution is nicer BUT for the time being, let's use the first (simpler).

        private readonly UmbracoContext _umbracoContext;
        private readonly IRuntimeState _runtimeState;

        private IRuntimeState RuntimeState => _runtimeState ?? Current.RuntimeState;

        private UmbracoContext UmbracoContext => _umbracoContext ?? Current.UmbracoContext;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="runtimeState"></param>
        public HttpInstallAuthorizeAttribute(UmbracoContext umbracoContext, IRuntimeState runtimeState)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            _umbracoContext = umbracoContext;
            _runtimeState = runtimeState;
        }

        public HttpInstallAuthorizeAttribute()
        { }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try
            {
                // if not configured (install or upgrade) then we can continue
                // otherwise we need to ensure that a user is logged in
                return RuntimeState.Level == RuntimeLevel.Install
                    || RuntimeState.Level == RuntimeLevel.Upgrade
                    || UmbracoContext.Security.ValidateCurrentUser();
            }
            catch (Exception ex)
            {
                Current.Logger.Error<HttpInstallAuthorizeAttribute>(ex, "An error occurred determining authorization");
                return false;
            }
        }
    }
}
