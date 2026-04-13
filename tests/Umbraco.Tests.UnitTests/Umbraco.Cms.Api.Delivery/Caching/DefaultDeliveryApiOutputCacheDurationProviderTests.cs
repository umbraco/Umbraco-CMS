using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Caching;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Caching;

[TestFixture]
public class DefaultDeliveryApiOutputCacheDurationProviderTests
{
    [Test]
    public void GetDuration_ReturnsNull()
    {
        var content = Mock.Of<IPublishedContent>();

        var provider = new DefaultDeliveryApiOutputCacheDurationProvider();

        Assert.That(provider.GetDuration(content), Is.Null);
    }
}
