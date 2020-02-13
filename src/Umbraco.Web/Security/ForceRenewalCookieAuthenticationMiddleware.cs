using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// This middleware is used simply to force renew the auth ticket if a flag to do so is found in the request
    /// </summary>
    internal class ForceRenewalCookieAuthenticationMiddleware : CookieAuthenticationMiddleware
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IRequestCache _requestCache;

        public ForceRenewalCookieAuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            UmbracoBackOfficeCookieAuthOptions options,
            IUmbracoContextAccessor umbracoContextAccessor,
            IRequestCache requestCache) : base(next, app, options)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _requestCache = requestCache;
        }

        protected override AuthenticationHandler<CookieAuthenticationOptions> CreateHandler()
        {
            return new ForceRenewalCookieAuthenticationHandler(_umbracoContextAccessor, _requestCache);
        }
    }
}
