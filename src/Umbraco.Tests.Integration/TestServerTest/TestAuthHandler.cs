using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.Security;
using Umbraco.Web.Common.Security;

namespace Umbraco.Tests.Integration.TestServerTest
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string TestAuthenticationScheme = "Test";

        private readonly IBackOfficeSignInManager _backOfficeSignInManager;

        private readonly BackOfficeIdentityUser _fakeUser;
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IBackOfficeSignInManager backOfficeSignInManager, IUserService userService, UmbracoMapper umbracoMapper)
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
            var ticket = new AuthenticationTicket(principal, TestAuthenticationScheme);

            return AuthenticateResult.Success(ticket);
        }
    }
}
