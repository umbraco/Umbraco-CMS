using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class PreviewServiceTests
{
    private const string Token = "token";

    [Test]
    public async Task TryEnterPreviewAsync_Sets_Secure_SameSiteNone_Cookie_On_Https_Request()
    {
        var cookieManagerMock = new Mock<ICookieManager>();
        var previewService = CreatePreviewService(cookieManagerMock, out var user, useHttps: false, requestUrl: "https://localhost/umbraco");

        var result = await previewService.TryEnterPreviewAsync(user);

        VerifyCookie(cookieManagerMock, secure: true, sameSiteMode: SameSiteMode.None);
        Assert.IsTrue(result);
    }

    [Test]
    public async Task TryEnterPreviewAsync_Sets_NonSecure_SameSiteLax_Cookie_On_Http_Request()
    {
        var cookieManagerMock = new Mock<ICookieManager>();
        var previewService = CreatePreviewService(cookieManagerMock, out var user, useHttps: false, requestUrl: "http://192.168.0.10:30645/umbraco");

        var result = await previewService.TryEnterPreviewAsync(user);

        VerifyCookie(cookieManagerMock, secure: false, sameSiteMode: SameSiteMode.Lax);
        Assert.IsTrue(result);
    }

    [Test]
    public async Task TryEnterPreviewAsync_Sets_Secure_SameSiteNone_Cookie_When_UseHttps_Is_Configured()
    {
        var cookieManagerMock = new Mock<ICookieManager>();
        var previewService = CreatePreviewService(cookieManagerMock, out var user, useHttps: true, requestUrl: "http://localhost/umbraco");

        var result = await previewService.TryEnterPreviewAsync(user);

        VerifyCookie(cookieManagerMock, secure: true, sameSiteMode: SameSiteMode.None);
        Assert.IsTrue(result);
    }

    [Test]
    public async Task TryEnterPreviewAsync_Sets_NonSecure_SameSiteLax_Cookie_When_Request_Url_Is_Unavailable()
    {
        var cookieManagerMock = new Mock<ICookieManager>();
        var previewService = CreatePreviewService(cookieManagerMock, out var user, useHttps: false, requestUrl: null);

        var result = await previewService.TryEnterPreviewAsync(user);

        VerifyCookie(cookieManagerMock, secure: false, sameSiteMode: SameSiteMode.Lax);
        Assert.IsTrue(result);
    }

    [Test]
    public async Task TryGetPreviewClaimsIdentityAsync_Succeeds_With_A_Valid_Token_On_A_Website_Request()
    {
        var previewService = CreatePreviewService(CreateCookieManagerWithPreviewToken(), out _, useHttps: true, requestUrl: "https://localhost/some/page");

        Attempt<ClaimsIdentity> attempt = await previewService.TryGetPreviewClaimsIdentityAsync();

        Assert.IsTrue(attempt.Success);
        Assert.IsNotNull(attempt.Result);
        Assert.IsTrue(previewService.IsInPreview());
    }

    [Test]
    public async Task TryGetPreviewClaimsIdentityAsync_Fails_When_The_Token_User_Cannot_Be_Resolved()
    {
        var previewService = CreatePreviewService(CreateCookieManagerWithPreviewToken(), out _, useHttps: true, requestUrl: "https://localhost/some/page", previewUserExists: false);

        Attempt<ClaimsIdentity> attempt = await previewService.TryGetPreviewClaimsIdentityAsync();

        Assert.IsFalse(attempt.Success);
        Assert.IsFalse(previewService.IsInPreview());
    }

    [Test]
    public async Task TryGetPreviewClaimsIdentityAsync_Fails_With_A_Valid_Token_On_A_Delivery_Api_Request()
    {
        var previewService = CreatePreviewService(CreateCookieManagerWithPreviewToken(), out _, useHttps: true, requestUrl: "https://localhost/umbraco/delivery/api/v2/content/item/1234");

        Attempt<ClaimsIdentity> attempt = await previewService.TryGetPreviewClaimsIdentityAsync();

        Assert.IsFalse(attempt.Success);
        Assert.IsFalse(previewService.IsInPreview());
    }

    private static Mock<ICookieManager> CreateCookieManagerWithPreviewToken()
    {
        var cookieManagerMock = new Mock<ICookieManager>();
        cookieManagerMock.Setup(x => x.GetCookieValue(Constants.Web.PreviewCookieName)).Returns(Token);
        return cookieManagerMock;
    }

    private static PreviewService CreatePreviewService(Mock<ICookieManager> cookieManagerMock, out IUser user, bool useHttps, string? requestUrl, bool previewUserExists = true)
    {
        var userKey = Guid.NewGuid();

#pragma warning disable CS0618 // Type or member is obsolete - all usage has been removed up in V19
        var previewTokenGeneratorMock = new Mock<IPreviewTokenGenerator>();
#pragma warning restore CS0618 // Type or member is obsolete
        previewTokenGeneratorMock
            .Setup(x => x.GenerateTokenAsync(It.Is<Guid>(y => y == userKey)))
            .ReturnsAsync(Attempt<string?>.Succeed(Token));
        previewTokenGeneratorMock
            .Setup(x => x.VerifyAsync(Token))
            .ReturnsAsync(Attempt.Succeed<Guid?>(userKey));

        var signInManagerMock = new Mock<ICoreBackOfficeSignInManager>();
        signInManagerMock
            .Setup(x => x.CreateUserPrincipalAsync(userKey))
            .ReturnsAsync(previewUserExists ? CreateBackOfficePrincipal(userKey) : null);
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ICoreBackOfficeSignInManager)))
            .Returns(signInManagerMock.Object);
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(Mock.Of<IServiceScope>(x => x.ServiceProvider == serviceProviderMock.Object));

        var hostingEnvironmentMock = new Mock<IHostingEnvironment>();
        hostingEnvironmentMock.SetupGet(x => x.ApplicationVirtualPath).Returns(string.Empty);
        hostingEnvironmentMock
            .Setup(x => x.ToAbsolute(It.IsAny<string>()))
            .Returns((string path) => path.TrimStart('~'));
        var umbracoRequestPaths = new UmbracoRequestPaths(
            hostingEnvironmentMock.Object,
            Options.Create(new UmbracoRequestPathsOptions()),
            Options.Create(new DeliveryApiSettings()));

        var requestAccessorMock = new Mock<IRequestAccessor>();
        requestAccessorMock
            .Setup(x => x.GetRequestUrl())
            .Returns(requestUrl is null ? null : new Uri(requestUrl));

        user = new UserBuilder()
            .WithKey(userKey)
            .Build();

        return new PreviewService(
            cookieManagerMock.Object,
            previewTokenGeneratorMock.Object,
            serviceScopeFactoryMock.Object,
            NoAppCache.Instance,
            Options.Create(new GlobalSettings { UseHttps = useHttps }),
            requestAccessorMock.Object,
            umbracoRequestPaths);
    }

    private static ClaimsPrincipal CreateBackOfficePrincipal(Guid userKey)
    {
        var identity = new ClaimsIdentity(Constants.Security.BackOfficeAuthenticationType);
        identity.AddRequiredClaims(
            "1",
            userKey,
            "test",
            "test",
            Enumerable.Empty<int>(),
            Enumerable.Empty<int>(),
            "en-US",
            Guid.NewGuid().ToString(),
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>());
        return new ClaimsPrincipal(identity);
    }

    private static void VerifyCookie(Mock<ICookieManager> cookieManagerMock, bool secure, SameSiteMode sameSiteMode)
        => cookieManagerMock
            .Verify(x => x.SetCookieValue(
                It.Is<string>(y => y == Constants.Web.PreviewCookieName),
                It.Is<string>(y => y == Token),
                It.Is<bool>(y => y == true),
                It.Is<bool>(y => y == secure),
                It.Is<string>(y => y == sameSiteMode.ToString())));
}
