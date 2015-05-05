using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Umbraco.Core;

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

            return UmbracoModule.ShouldAuthenticateRequest(
                context.HttpContextFromOwinContext().Request, 
                _umbracoContextAccessor.Value.OriginalRequestUrl) == false 
                //Don't auth request, don't return a cookie
                ? null 
                //Return the default implementation
                : base.GetRequestCookie(context, key);
        }
    }
}