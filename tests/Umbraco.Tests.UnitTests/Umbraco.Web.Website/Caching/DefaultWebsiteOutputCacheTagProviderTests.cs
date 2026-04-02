using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Website.Caching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Caching;

[TestFixture]
public class DefaultWebsiteOutputCacheTagProviderTests
{
    [Test]
    public void GetTags_ReturnsContentTypeTag()
    {
        var contentType = Mock.Of<IPublishedContentType>(ct => ct.Alias == "blogPost");
        var content = Mock.Of<IPublishedContent>(c => c.ContentType == contentType);

        var provider = new DefaultWebsiteOutputCacheTagProvider();

        var tags = provider.GetTags(content).ToList();

        Assert.That(tags, Has.Count.EqualTo(1));
        Assert.That(tags[0], Is.EqualTo("umb-content-type-blogPost"));
    }
}
