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

/// <summary>
/// Contains unit tests for the <see cref="BackOfficeExternalLoginService"/> class, verifying its authentication and login-related functionality in the backoffice context.
/// </summary>
[TestFixture]
public partial class BackOfficeExternalLoginServiceTests
{
    private class BackOfficeExternalLoginServiceSetup
    {
        private BackOfficeExternalLoginService? _sut;

    /// <summary>
    /// Gets the system under test (SUT) instance of <see cref="BackOfficeExternalLoginService"/>.
    /// </summary>
        public BackOfficeExternalLoginService Sut =>
            _sut ??= new BackOfficeExternalLoginService(
                BackOfficeLoginProviders.Object,
                UserService.Object,
                BackOfficeUserManager.Object,
                BackOfficeSignInManager.Object,
                MemoryCache.Object);

    /// <summary>
    /// Gets the mock instance of the back office external login providers.
    /// </summary>
        public Mock<IBackOfficeExternalLoginProviders> BackOfficeLoginProviders { get; } = new();

    /// <summary>
    /// Gets the mock instance of IUserService used for testing.
    /// </summary>
        public Mock<IUserService> UserService { get; } = new();

    /// <summary>
    /// Gets the mock instance of <see cref="IBackOfficeUserManager"/> used for testing.
    /// </summary>
        public Mock<IBackOfficeUserManager> BackOfficeUserManager { get; } = new();

    /// <summary>
    /// Gets the mock instance of <see cref="IBackOfficeSignInManager"/> used for testing.
    /// </summary>
        public Mock<IBackOfficeSignInManager> BackOfficeSignInManager { get; } = new();

    /// <summary>
    /// Gets the mock memory cache used for testing.
    /// </summary>
        public Mock<IMemoryCache> MemoryCache { get; } = new();
    }

    private class MockAuthenticationHandler : IAuthenticationHandler
    {
    /// <summary>
    /// Initializes the authentication handler with the specified scheme and HTTP context.
    /// </summary>
    /// <param name="scheme">The authentication scheme to initialize with.</param>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) =>
            throw new NotImplementedException();

    /// <summary>
    /// Authenticates the user asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous authentication operation. The task result contains the authentication result.</returns>
        public Task<AuthenticateResult> AuthenticateAsync() => throw new NotImplementedException();

    /// <summary>
    /// Challenges the current authentication with the specified authentication properties.
    /// </summary>
    /// <param name="properties">The authentication properties to use during the challenge.</param>
    /// <returns>A task that represents the asynchronous challenge operation.</returns>
        public Task ChallengeAsync(AuthenticationProperties? properties) => throw new NotImplementedException();

    /// <summary>
    /// Forbids the authentication process with the specified authentication properties.
    /// </summary>
    /// <param name="properties">The authentication properties to use when forbidding the authentication.</param>
    /// <returns>A task that represents the asynchronous forbid operation.</returns>
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
