using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ContentBuilderTests : DeliveryApiTests
{
    [Test]
    public void ContentBuilder_MapsContentDataAndPropertiesCorrectly()
    {
        var content = new Mock<IPublishedContent>();

        var prop1 = new PublishedElementPropertyBase(DeliveryApiPropertyType, content.Object, false, PropertyCacheLevel.None, Mock.Of<ICacheManager>());
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None, Mock.Of<ICacheManager>());

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var key = Guid.NewGuid();
        var urlSegment = "url-segment";
        var name = "The page";
        ConfigurePublishedContentMock(content, key, name, urlSegment, contentType.Object, new[] { prop1, prop2 });
        content.SetupGet(c => c.CreateDate).Returns(new DateTime(2023, 06, 01));
        content.SetupGet(c => c.UpdateDate).Returns(new DateTime(2023, 07, 12));

        var apiContentRouteProvider = new Mock<IApiContentPathProvider>();
        apiContentRouteProvider
            .Setup(p => p.GetContentPath(It.IsAny<IPublishedContent>(), It.IsAny<string?>()))
            .Returns((IPublishedContent c, string? culture) => $"url:{c.UrlSegment}");

        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        IEnumerable<Guid> ancestorsKeys = [];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(key, out ancestorsKeys)).Returns(true);

        var routeBuilder = CreateContentRouteBuilder(apiContentRouteProvider.Object, CreateGlobalSettings(), navigationQueryService: navigationQueryServiceMock.Object);

        var builder = new ApiContentBuilder(new ApiContentNameProvider(), routeBuilder, CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(content.Object);

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Name);
        Assert.AreEqual("thePageType", result.ContentType);
        Assert.AreEqual("/url:url-segment", result.Route.Path);
        Assert.AreEqual(key, result.Id);
        Assert.AreEqual(2, result.Properties.Count);
        Assert.AreEqual("Delivery API value", result.Properties["deliveryApi"]);
        Assert.AreEqual("Default value", result.Properties["default"]);
        Assert.AreEqual(new DateTime(2023, 06, 01), result.CreateDate);
        Assert.AreEqual(new DateTime(2023, 07, 12), result.UpdateDate);
    }

    [Test]
    public void ContentBuilder_CanCustomizeContentNameInDeliveryApiOutput()
    {
        var content = new Mock<IPublishedContent>();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        ConfigurePublishedContentMock(content, Guid.NewGuid(), "The page", "the-page", contentType.Object, Array.Empty<PublishedElementPropertyBase>());

        var customNameProvider = new Mock<IApiContentNameProvider>();
        customNameProvider.Setup(n => n.GetName(content.Object)).Returns($"Custom name for: {content.Object.Name}");

        var routeBuilder = new Mock<IApiContentRouteBuilder>();
        routeBuilder
            .Setup(r => r.Build(content.Object, It.IsAny<string?>()))
            .Returns(new ApiContentRoute(content.Object.UrlSegment!, new ApiContentStartItem(Guid.NewGuid(), "/")));

        var builder = new ApiContentBuilder(customNameProvider.Object, routeBuilder.Object, CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(content.Object);

        Assert.NotNull(result);
        Assert.AreEqual("Custom name for: The page", result.Name);
    }

    [Test]
    public void ContentBuilder_ReturnsNullForUnRoutableContent()
    {
        var content = new Mock<IPublishedContent>();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        ConfigurePublishedContentMock(content, Guid.NewGuid(), "The page", "the-page", contentType.Object, Array.Empty<PublishedElementPropertyBase>());

        var routeBuilder = new Mock<IApiContentRouteBuilder>();
        routeBuilder
            .Setup(r => r.Build(content.Object, It.IsAny<string?>()))
            .Returns((ApiContentRoute)null);

        var builder = new ApiContentBuilder(new ApiContentNameProvider(), routeBuilder.Object, CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(content.Object);

        Assert.Null(result);
    }
}
