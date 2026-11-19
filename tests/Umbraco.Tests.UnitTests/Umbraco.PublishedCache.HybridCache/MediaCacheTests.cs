using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
internal sealed class MediaCacheTests
{
    [Test]
    public void Can_Get_Several_Media_Items_With_One_Batched_Read()
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent[] media = [Mock.Of<IPublishedContent>(), Mock.Of<IPublishedContent>()];
        var mediaCacheService = new Mock<IMediaCacheService>();
        mediaCacheService.Setup(x => x.GetByKeys(keys)).Returns(media);
        var sut = new MediaCache(
            mediaCacheService.Object,
            Mock.Of<IMediaNavigationQueryService>());

        IReadOnlyList<IPublishedContent> result = sut.GetByIds(keys);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(media));
            mediaCacheService.Verify(x => x.GetByKeys(keys), Times.Once);
            mediaCacheService.Verify(x => x.GetByKey(It.IsAny<Guid>()), Times.Never);
        });
    }
}
