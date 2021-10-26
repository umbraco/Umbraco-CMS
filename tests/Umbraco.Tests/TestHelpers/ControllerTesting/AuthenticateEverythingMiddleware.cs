using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using Umbraco.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    /// <summary>
    /// Ensures there's an admin user assigned to the request
    /// </summary>
    public class AuthenticateEverythingMiddleware : AuthenticationMiddleware<AuthenticationOptions>
    {
        public AuthenticateEverythingMiddleware(OwinMiddleware next, IAppBuilder app, AuthenticationOptions options)
            : base(next, options)
        {
        }

        protected override AuthenticationHandler<AuthenticationOptions> CreateHandler()
        {
            return new AuthenticateEverythingHandler();
        }

        public class AuthenticateEverythingHandler : AuthenticationHandler<AuthenticationOptions>
        {
            protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
            {
                var securityStamp = Guid.NewGuid().ToString();

                var identity = new ClaimsIdentity();
                identity.AddRequiredClaims(
                    Cms.Core.Constants.Security.SuperUserIdAsString,
                    "admin",
                    "Admin",
                    new[] { -1 },
                    new[] { -1 },
                    "en-US",
                    securityStamp,
                    new[] { "content", "media", "members" },
                    new[] { "admin" });

                return Task.FromResult(new AuthenticationTicket(
                    identity,
                    new AuthenticationProperties()
                    {
                        ExpiresUtc = DateTime.Now.AddDays(1)
                    }));
            }
        }

        public class AuthenticateEverythingAuthenticationOptions : AuthenticationOptions
        {
            public AuthenticateEverythingAuthenticationOptions()
                : base("AuthenticateEverything")
            {
                AuthenticationMode = AuthenticationMode.Active;
            }
        }
    }
}
