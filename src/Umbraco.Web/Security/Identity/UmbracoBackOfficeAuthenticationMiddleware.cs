using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Used to enable the normal Umbraco back office authentication to operate
    /// </summary>
    public class UmbracoBackOfficeAuthenticationMiddleware : AuthenticationMiddleware<UmbracoBackOfficeAuthenticationOptions>
    {
        private readonly ISecuritySection _securitySection;

        public UmbracoBackOfficeAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, UmbracoBackOfficeAuthenticationOptions options, ISecuritySection securitySection)
            : base(next, options)
        {
            _securitySection = securitySection;
        }

        protected override AuthenticationHandler<UmbracoBackOfficeAuthenticationOptions> CreateHandler()
        {
            return new UmbracoBackOfficeAuthenticationHandler(_securitySection);
        }
    }
}