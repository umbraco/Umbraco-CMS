using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Security;
using Umbraco.Web.UI.NetCore;

namespace Umbraco.Tests.Integration.TestServerTest
{
    public class UmbracoWebApplicationFactory : CustomWebApplicationFactory<Startup>
    {
        public UmbracoWebApplicationFactory(string testDbConnectionString) :base(testDbConnectionString)
        {
        }
    }

    public abstract class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _testDbConnectionString;

        protected CustomWebApplicationFactory(string testDbConnectionString)
        {
            _testDbConnectionString = testDbConnectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(services =>
            {
               services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => {});
            });

            builder.ConfigureAppConfiguration(x =>
            {
                x.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["ConnectionStrings:"+ Constants.System.UmbracoConnectionName] = _testDbConnectionString
                });
            });

        }

        protected override IHostBuilder CreateHostBuilder()
        {

            var builder = base.CreateHostBuilder();
            builder.UseUmbraco();
            return builder;
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly BackOfficeSignInManager _backOfficeSignInManager;

        private readonly BackOfficeIdentityUser _fakeUser;
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, BackOfficeSignInManager backOfficeSignInManager, IUserService userService, UmbracoMapper umbracoMapper)
            : base(options, logger, encoder, clock)
        {
            _backOfficeSignInManager = backOfficeSignInManager;

            var user = userService.GetUserById(Constants.Security.SuperUserId);
            _fakeUser = umbracoMapper.Map<IUser, BackOfficeIdentityUser>(user);
            _fakeUser.SecurityStamp = "Needed";
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            var principal = await _backOfficeSignInManager.CreateUserPrincipalAsync(_fakeUser);
            var ticket = new AuthenticationTicket(principal, "Test");

            return AuthenticateResult.Success(ticket);
        }
    }
}
