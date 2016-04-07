using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Umbraco.Core;
using Umbraco.Core.Configuration;
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
        private readonly string[] _explicitPaths;
        private readonly string _getRemainingSecondsPath;

        public BackOfficeCookieManager(IUmbracoContextAccessor umbracoContextAccessor)
            : this(umbracoContextAccessor, null)
        {
            
        }
        public BackOfficeCookieManager(IUmbracoContextAccessor umbracoContextAccessor, IEnumerable<string> explicitPaths)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _explicitPaths = explicitPaths == null ? null : explicitPaths.ToArray();
            _getRemainingSecondsPath = string.Format("{0}/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds", GlobalSettings.Path);
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
            if (_umbracoContextAccessor.Value.Application.IsConfigured == false
                && _umbracoContextAccessor.Value.Application.DatabaseContext.IsDatabaseConfigured == false)
            {
                //Do not authenticate the request if we don't have a db and we are not configured - since we will never need
                // to know a current user in this scenario - we treat it as a new install. Without this we can have some issues
                // when people have older invalid cookies on the same domain since our user managers might attempt to lookup a user
                // and we don't even have a db.
                return false;
            }

            var request = ctx.Request;
            var httpCtx = ctx.TryGetHttpContext();
            
            //check the explicit paths
            if (_explicitPaths != null)
            {
                return _explicitPaths.Any(x => x.InvariantEquals(request.Uri.AbsolutePath));
            }
            
            //check user seconds path
            if (request.Uri.AbsolutePath.InvariantEquals(_getRemainingSecondsPath)) return false;

            if (//check the explicit flag
                (checkForceAuthTokens && ctx.Get<bool?>("umbraco-force-auth") != null)
                || (checkForceAuthTokens && httpCtx.Success && httpCtx.Result.Items["umbraco-force-auth"] != null)                
                //check back office
                || request.Uri.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath)
                //check installer
                || request.Uri.IsInstallerRequest()                
                //check for base
                || BaseRest.BaseRestHandler.IsBaseRestRequest(originalRequestUrl))
            {
                return true;
            }
            return false;
        }

    }
}