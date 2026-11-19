using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
internal sealed class DocumentCacheTests
{
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Can_Get_Several_Documents_With_One_Batched_Read(bool preview)
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent[] documents = [Mock.Of<IPublishedContent>(), Mock.Of<IPublishedContent>()];
        var documentCacheService = new Mock<IDocumentCacheService>();
        documentCacheService.Setup(x => x.GetByKeys(keys, preview)).Returns(documents);
        var sut = new DocumentCache(
            documentCacheService.Object,
            Mock.Of<IPublishedContentTypeCache>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IDocumentUrlService>(),
            new Lazy<IPublishedUrlProvider>(() => Mock.Of<IPublishedUrlProvider>()));

        IReadOnlyList<IPublishedContent> result = sut.GetByIds(preview, keys);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(documents));
            documentCacheService.Verify(x => x.GetByKeys(keys, preview), Times.Once);
            documentCacheService.Verify(x => x.GetByKey(It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
        });
    }
}
