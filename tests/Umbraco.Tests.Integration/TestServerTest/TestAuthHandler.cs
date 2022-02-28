// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.TestServerTest
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string TestAuthenticationScheme = "Test";

        private readonly IBackOfficeSignInManager _backOfficeSignInManager;

        private readonly BackOfficeIdentityUser _fakeUser;

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IBackOfficeSignInManager backOfficeSignInManager,
            IUserService userService,
            IUmbracoMapper umbracoMapper)
            : base(options, logger, encoder, clock)
        {
            _backOfficeSignInManager = backOfficeSignInManager;

            IUser user = userService.GetUserById(Constants.Security.SuperUserId);
            _fakeUser = umbracoMapper.Map<IUser, BackOfficeIdentityUser>(user);
            _fakeUser.SecurityStamp = "Needed";
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            ClaimsPrincipal principal = await _backOfficeSignInManager.CreateUserPrincipalAsync(_fakeUser);
            var ticket = new AuthenticationTicket(principal, TestAuthenticationScheme);

            return AuthenticateResult.Success(ticket);
        }
    }
}
