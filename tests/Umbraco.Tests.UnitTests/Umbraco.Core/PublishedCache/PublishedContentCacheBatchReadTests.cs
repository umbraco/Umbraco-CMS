using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PublishedCache;

[TestFixture]
public class PublishedContentCacheBatchReadTests
{
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Can_Get_Several_Content_Items_In_The_Order_Of_The_Keys_And_Skip_Missing_Ones_By_Default(bool preview)
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent first = Mock.Of<IPublishedContent>();
        IPublishedContent third = Mock.Of<IPublishedContent>();
        var cache = new Mock<IPublishedContentCache> { CallBase = true };
        cache.Setup(x => x.GetById(preview, keys[0])).Returns(first);
        cache.Setup(x => x.GetById(preview, keys[2])).Returns(third);

        IReadOnlyList<IPublishedContent> result = cache.Object.GetByIds(preview, keys);

        Assert.That(result, Is.EqualTo(new[] { first, third }));
    }

    [Test]
    public void Cannot_Get_Content_Items_Without_Keys_By_Default()
    {
        var cache = new Mock<IPublishedContentCache> { CallBase = true };

        Assert.IsEmpty(cache.Object.GetByIds(false, []));
    }
}
