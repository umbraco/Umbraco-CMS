using System;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// A custom cookie manager that is used to read the cookie from the request.
    /// </summary>
    /// <remarks>
    /// Umbraco's back office cookie needs to be read on two paths: /umbraco and /install and /base therefore we cannot just set the cookie path to be /umbraco, 
    /// instead we'll specify our own cookie manager and return null if the request isn't for an acceptable path.
    /// </remarks>
    internal class BackOfficeCookieManager : ChunkingCookieManager, ICookieManager
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public BackOfficeCookieManager(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Explicitly implement this so that we filter the request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string ICookieManager.GetRequestCookie(IOwinContext context, string key)
        {
            if (_umbracoContextAccessor.Value == null || context.Request.Uri.IsClientSideRequest())
            {
                return null;
            }
            
            return ShouldAuthenticateRequest(
                context, 
                _umbracoContextAccessor.Value.OriginalRequestUrl) == false 
                //Don't auth request, don't return a cookie
                ? null 
                //Return the default implementation
                : base.GetRequestCookie(context, key);
        }

        /// <summary>
        /// Determines if we should authenticate the request
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="originalRequestUrl"></param>
        /// <param name="checkForceAuthTokens"></param>
        /// <returns></returns>
        /// <remarks>
        /// We auth the request when:
        /// * it is a back office request
        /// * it is an installer request
        /// * it is a /base request
        /// * it is a preview request
        /// </remarks>
        internal bool ShouldAuthenticateRequest(IOwinContext ctx, Uri originalRequestUrl, bool checkForceAuthTokens = true)
        {
            var request = ctx.Request;
            var httpCtx = ctx.TryGetHttpContext();
            
            if (//check the explicit flag
                (checkForceAuthTokens && ctx.Get<bool?>("umbraco-force-auth") != null)
                || (checkForceAuthTokens && httpCtx.Success && httpCtx.Result.Items["umbraco-force-auth"] != null)
                //check back office
                || request.Uri.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath)
                //check installer
                || request.Uri.IsInstallerRequest()
                //detect in preview
                || (request.HasPreviewCookie() && request.Uri != null && request.Uri.AbsolutePath.StartsWith(IOHelper.ResolveUrl(SystemDirectories.Umbraco)) == false)
                //check for base
                || BaseRest.BaseRestHandler.IsBaseRestRequest(originalRequestUrl))
            {
                return true;
            }
            return false;
        }

    }
}