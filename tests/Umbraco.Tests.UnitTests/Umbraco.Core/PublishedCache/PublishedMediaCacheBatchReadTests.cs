using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PublishedCache;

[TestFixture]
public class PublishedMediaCacheBatchReadTests
{
    [Test]
    public void Can_Get_Several_Media_Items_In_The_Order_Of_The_Keys_And_Skip_Missing_Ones_By_Default()
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent second = Mock.Of<IPublishedContent>();
        IPublishedContent third = Mock.Of<IPublishedContent>();
        var cache = new Mock<IPublishedMediaCache> { CallBase = true };
        cache.Setup(x => x.GetById(keys[1])).Returns(second);
        cache.Setup(x => x.GetById(keys[2])).Returns(third);

        IReadOnlyList<IPublishedContent> result = cache.Object.GetByIds(keys);

        Assert.That(result, Is.EqualTo(new[] { second, third }));
    }

    [Test]
    public void Cannot_Get_Media_Items_Without_Keys_By_Default()
    {
        var cache = new Mock<IPublishedMediaCache> { CallBase = true };

        Assert.IsEmpty(cache.Object.GetByIds([]));
    }
}
