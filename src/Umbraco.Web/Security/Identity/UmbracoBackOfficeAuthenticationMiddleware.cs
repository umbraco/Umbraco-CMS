using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Used to enable the normal Umbraco back office authentication to operate
    /// </summary>
    public class UmbracoBackOfficeAuthenticationMiddleware : AuthenticationMiddleware<UmbracoBackOfficeCookieAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public UmbracoBackOfficeAuthenticationMiddleware(
            OwinMiddleware next, 
            IAppBuilder app, 
            UmbracoBackOfficeCookieAuthenticationOptions options,
            ILogger logger)
            : base(next, options)
        {
            _logger = logger;
        }

        protected override AuthenticationHandler<UmbracoBackOfficeCookieAuthenticationOptions> CreateHandler()
        {
            return new UmbracoBackOfficeAuthenticationHandler(_logger);
        }
    }
}