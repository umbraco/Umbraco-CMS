using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class PreviewServiceTests
{
    [Test]
    public async Task TryEnterPreviewAsync_Sets_Expected_Cookie_On_Successful_Token_Generation()
    {
        var userKey = Guid.NewGuid();
        const string Token = "token";

        var cookieManagerMock = new Mock<ICookieManager>();

        var previewTokenGeneratorMock = new Mock<IPreviewTokenGenerator>();
        previewTokenGeneratorMock
            .Setup(x => x.GenerateTokenAsync(It.Is<Guid>(y => y == userKey)))
            .ReturnsAsync(Attempt<string?>.Succeed(Token));

        var previewService = new PreviewService(
            cookieManagerMock.Object,
            previewTokenGeneratorMock.Object,
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<IRequestCache>());

        var user = new UserBuilder()
            .WithKey(userKey)
            .Build();

        var result = await previewService.TryEnterPreviewAsync(user);

        cookieManagerMock
            .Verify(x => x.SetCookieValue(
                It.Is<string>(y => y == Constants.Web.PreviewCookieName),
                It.Is<string>(y => y == Token),
                It.Is<bool>(y => y == true),
                It.Is<bool>(y => y == true),
                It.Is<string>(y => y == SameSiteMode.None.ToString())));

        Assert.IsTrue(result);
    }
}
