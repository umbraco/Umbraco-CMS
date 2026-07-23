using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

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

    private static PreviewService CreatePreviewService(Mock<ICookieManager> cookieManagerMock, out IUser user, bool useHttps, string? requestUrl)
    {
        var userKey = Guid.NewGuid();

        var previewTokenGeneratorMock = new Mock<IPreviewTokenGenerator>();
        previewTokenGeneratorMock
            .Setup(x => x.GenerateTokenAsync(It.Is<Guid>(y => y == userKey)))
            .ReturnsAsync(Attempt<string?>.Succeed(Token));

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
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<IRequestCache>(),
            Options.Create(new GlobalSettings { UseHttps = useHttps }),
            requestAccessorMock.Object);
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
