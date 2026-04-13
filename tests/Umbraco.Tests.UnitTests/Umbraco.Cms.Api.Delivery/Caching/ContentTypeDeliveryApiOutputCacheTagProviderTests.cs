using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Caching;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Caching;

[TestFixture]
public class ContentTypeDeliveryApiOutputCacheTagProviderTests
{
    [Test]
    public void GetTags_ReturnsContentTypeTag()
    {
        var contentType = Mock.Of<IPublishedContentType>(ct => ct.Alias == "blogPost");
        var content = Mock.Of<IPublishedContent>(c => c.ContentType == contentType);

        var provider = new ContentTypeDeliveryApiOutputCacheTagProvider();

        var tags = provider.GetTags(content).ToList();

        Assert.That(tags, Has.Count.EqualTo(1));
        Assert.That(tags[0], Is.EqualTo("umb-dapi-content-type-blogPost"));
    }
}
