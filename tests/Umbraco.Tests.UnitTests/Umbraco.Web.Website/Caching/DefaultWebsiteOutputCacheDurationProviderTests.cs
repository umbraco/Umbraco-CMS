using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Website.Caching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Caching;

[TestFixture]
public class DefaultWebsiteOutputCacheDurationProviderTests
{
    [Test]
    public void GetDuration_ReturnsNull()
    {
        var content = Mock.Of<IPublishedContent>();

        var provider = new DefaultWebsiteOutputCacheDurationProvider();

        Assert.That(provider.GetDuration(content), Is.Null);
    }
}
