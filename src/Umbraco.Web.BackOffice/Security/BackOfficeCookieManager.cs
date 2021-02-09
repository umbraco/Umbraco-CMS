using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// A custom cookie manager that is used to read the cookie from the request.
    /// </summary>
    /// <remarks>
    /// Umbraco's back office cookie needs to be read on two paths: /umbraco and /install, therefore we cannot just set the cookie path to be /umbraco,
    /// instead we'll specify our own cookie manager and return null if the request isn't for an acceptable path.
    /// </remarks>
    public class BackOfficeCookieManager : ChunkingCookieManager, Microsoft.AspNetCore.Authentication.Cookies.ICookieManager
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IRuntimeState _runtime;
        private readonly string[] _explicitPaths;
        private readonly UmbracoRequestPaths _umbracoRequestPaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeCookieManager"/> class.
        /// </summary>
        public BackOfficeCookieManager(
            IUmbracoContextAccessor umbracoContextAccessor,
            IRuntimeState runtime,
            UmbracoRequestPaths umbracoRequestPaths)
            : this(umbracoContextAccessor, runtime, null, umbracoRequestPaths)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeCookieManager"/> class.
        /// </summary>
        public BackOfficeCookieManager(
            IUmbracoContextAccessor umbracoContextAccessor,
            IRuntimeState runtime,
            IEnumerable<string> explicitPaths,
            UmbracoRequestPaths umbracoRequestPaths)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _runtime = runtime;
            _explicitPaths = explicitPaths?.ToArray();
            _umbracoRequestPaths = umbracoRequestPaths;
        }

        /// <summary>
        /// Determines if we should authenticate the request
        /// </summary>
        /// <returns>true if the request should be authenticated</returns>
        /// <remarks>
        /// We auth the request when:
        /// * it is a back office request
        /// * it is an installer request
        /// * it is a preview request
        /// </remarks>
        public bool ShouldAuthenticateRequest(string absPath)
        {
            // Do not authenticate the request if we are not running (don't have a db, are not configured) - since we will never need
            // to know a current user in this scenario - we treat it as a new install. Without this we can have some issues
            // when people have older invalid cookies on the same domain since our user managers might attempt to lookup a user
            // and we don't even have a db.
            // was: app.IsConfigured == false (equiv to !Run) && dbContext.IsDbConfigured == false (equiv to Install)
            // so, we handle .Install here and NOT .Upgrade
            if (_runtime.Level == RuntimeLevel.Install)
            {
                return false;
            }

            // check the explicit paths
            if (_explicitPaths != null)
            {
                return _explicitPaths.Any(x => x.InvariantEquals(absPath));
            }

            if (// check back office
                _umbracoRequestPaths.IsBackOfficeRequest(absPath)

                // check installer
                || _umbracoRequestPaths.IsInstallerRequest(absPath))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Explicitly implement this so that we filter the request
        /// </summary>
        /// <inheritdoc/>
        string Microsoft.AspNetCore.Authentication.Cookies.ICookieManager.GetRequestCookie(HttpContext context, string key)
        {
            var absPath = context.Request.Path;

            if (_umbracoContextAccessor.UmbracoContext == null || _umbracoRequestPaths.IsClientSideRequest(absPath))
            {
                return null;
            }

            return ShouldAuthenticateRequest(absPath) == false

                // Don't auth request, don't return a cookie
                ? null

                // Return the default implementation
                : GetRequestCookie(context, key);
        }

    }
}
