using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class DocumentUrlFactoryTests
{
    private static (DocumentUrlFactory Factory, Mock<IPublishedUrlInfoProvider> Provider) CreateFactory()
    {
        var provider = new Mock<IPublishedUrlInfoProvider>();

        // Return a single url tagged with the requested culture so assertions can distinguish calls.
        provider
            .Setup(x => x.GetAllAsync(It.IsAny<IContent>(), It.IsAny<string?>()))
            .ReturnsAsync((IContent _, string? culture) =>
                new HashSet<UrlInfo> { UrlInfo.AsUrl($"/{culture ?? "all"}/", "test", culture) });

        var factory = new DocumentUrlFactory(
            provider.Object,
            new UrlProviderCollection(() => []),
            Mock.Of<IPreviewService>(),
            Mock.Of<IBackOfficeSecurityAccessor>(),
            NullLogger<DocumentUrlFactory>.Instance);

        return (factory, provider);
    }

    private static IContent CreateContent(Guid? key = null)
    {
        var content = new Mock<IContent>();
        content.SetupGet(x => x.Key).Returns(key ?? Guid.NewGuid());
        return content.Object;
    }

    [Test]
    public async Task Can_Pass_Requested_Culture_To_Provider()
    {
        var (factory, provider) = CreateFactory();
        var content = CreateContent();

        var result = (await factory.CreateUrlsAsync(content, "da-DK")).ToList();

        provider.Verify(x => x.GetAllAsync(content, "da-DK"), Times.Once);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("da-DK", result[0].Culture);
    }

    [Test]
    public async Task Can_Request_All_Cultures_When_No_Culture_Provided()
    {
        var (factory, provider) = CreateFactory();
        var content = CreateContent();

        await factory.CreateUrlsAsync(content);

        // The parameterless overload must delegate with a null culture (all cultures).
        provider.Verify(x => x.GetAllAsync(content, null), Times.Once);
    }

    [Test]
    public async Task Can_Pass_Requested_Culture_For_Every_Item()
    {
        var (factory, provider) = CreateFactory();
        var contentA = CreateContent();
        var contentB = CreateContent();

        var result = (await factory.CreateUrlSetsAsync([contentA, contentB], "da-DK")).ToList();

        provider.Verify(x => x.GetAllAsync(contentA, "da-DK"), Times.Once);
        provider.Verify(x => x.GetAllAsync(contentB, "da-DK"), Times.Once);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(contentA.Key, result[0].Id);
        Assert.AreEqual(contentB.Key, result[1].Id);
    }

    [Test]
    public async Task Can_Request_All_Cultures_For_Url_Sets_When_No_Culture_Provided()
    {
        var (factory, provider) = CreateFactory();
        var content = CreateContent();

        await factory.CreateUrlSetsAsync([content]);

        provider.Verify(x => x.GetAllAsync(content, null), Times.Once);
    }
}
