using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// This middleware is used simply to force renew the auth ticket if a flag to do so is found in the request
    /// </summary>
    internal class ForceRenewalCookieAuthenticationMiddleware : CookieAuthenticationMiddleware
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public ForceRenewalCookieAuthenticationMiddleware(
            OwinMiddleware next, 
            IAppBuilder app, 
            UmbracoBackOfficeCookieAuthOptions options,
            IUmbracoContextAccessor umbracoContextAccessor) : base(next, app, options)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        protected override AuthenticationHandler<CookieAuthenticationOptions> CreateHandler()
        {
            return new ForceRenewalCookieAuthenticationHandler(_umbracoContextAccessor);
        }
    }
}