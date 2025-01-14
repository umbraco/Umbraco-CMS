using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
public partial class BackOfficeExternalLoginServiceTests
{
    private class BackOfficeExternalLoginServiceSetup
    {
        private BackOfficeExternalLoginService? _sut;

        public BackOfficeExternalLoginService Sut =>
            _sut ??= new BackOfficeExternalLoginService(
                BackOfficeLoginProviders.Object,
                UserService.Object,
                BackOfficeUserManager.Object,
                BackOfficeSignInManager.Object,
                MemoryCache.Object);

        public Mock<IBackOfficeExternalLoginProviders> BackOfficeLoginProviders { get; } = new();

        public Mock<IUserService> UserService { get; } = new();

        public Mock<IBackOfficeUserManager> BackOfficeUserManager { get; } = new();

        public Mock<IBackOfficeSignInManager> BackOfficeSignInManager { get; } = new();

        public Mock<IMemoryCache> MemoryCache { get; } = new();
    }

    private class MockAuthenticationHandler : IAuthenticationHandler
    {
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) =>
            throw new NotImplementedException();

        public Task<AuthenticateResult> AuthenticateAsync() => throw new NotImplementedException();

        public Task ChallengeAsync(AuthenticationProperties? properties) => throw new NotImplementedException();

        public Task ForbidAsync(AuthenticationProperties? properties) => throw new NotImplementedException();
    }

    private BackOfficeExternaLoginProviderScheme TestExternalLoginProviderScheme(
        string providerName,
        bool allowManualLinking,
        AuthenticationScheme? authScheme = null) => new BackOfficeExternaLoginProviderScheme(
        new BackOfficeExternalLoginProvider(
            providerName,
            new TestOptionsMonitor<BackOfficeExternalLoginProviderOptions>(
                new BackOfficeExternalLoginProviderOptions(
                    new ExternalSignInAutoLinkOptions(allowManualLinking: allowManualLinking)))),
        authScheme ?? TestAuthenticationScheme());

    private AuthenticationScheme TestAuthenticationScheme(string name = "test", string displayName = "test") =>
        new AuthenticationScheme(name, displayName, typeof(MockAuthenticationHandler));
}
