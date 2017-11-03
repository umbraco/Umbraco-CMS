using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using Umbraco.Core.Security;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
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
                var sessionId = Guid.NewGuid().ToString();
                var identity = new UmbracoBackOfficeIdentity(
                    new UserData(sessionId)
                    {
                        SecurityStamp = sessionId,
                        Id = 0,
                        Roles = new[] { "admin" },
                        AllowedApplications = new[] { "content", "media", "members" },
                        Culture = "en-US",
                        RealName = "Admin",                        
                        Username = "admin"
                    });

                return Task.FromResult(new AuthenticationTicket(identity,
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