using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ContentBuilderTests : DeliveryApiTests
{
    [Test]
    public void ContentBuilder_MapsContentDataAndPropertiesCorrectly()
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);

        var propertyData = new PropertyData { Value = "n/a", Culture = "abc", Segment = string.Empty };

        var prop1 = new PublishedProperty(DeliveryApiPropertyType, content.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);
        var prop2 = new PublishedProperty(DefaultPropertyType, content.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);

        var key = Guid.NewGuid();
        var urlSegment = "url-segment";
        var name = "The page";
        ConfigurePublishedContentMock(content, key, name, contentType.Object, new[] { prop1, prop2 });
        content.SetupGet(c => c.CreateDate).Returns(new DateTime(2023, 06, 01));
        content.SetupGet(c => c.UpdateDate).Returns(new DateTime(2023, 07, 12));

        var apiContentRouteProvider = new Mock<IApiContentPathProvider>();
        apiContentRouteProvider
            .Setup(p => p.GetContentPath(It.IsAny<IPublishedContent>(), It.IsAny<string?>()))
            .Returns((IPublishedContent c, string? culture) => $"url:{urlSegment}");

        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        IEnumerable<Guid> ancestorsKeys = [];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(key, out ancestorsKeys)).Returns(true);

        var routeBuilder = CreateContentRouteBuilder(apiContentRouteProvider.Object, CreateGlobalSettings(), navigationQueryService: navigationQueryServiceMock.Object);

        var builder = new ApiContentBuilder(new ApiContentNameProvider(), routeBuilder, CreateOutputExpansionStrategyAccessor(), CreateVariationContextAccessor());
        var result = builder.Build(content.Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("The page"));
        Assert.That(result.ContentType, Is.EqualTo("thePageType"));
        Assert.That(result.Route.Path, Is.EqualTo("/url:url-segment/"));
        Assert.That(result.Id, Is.EqualTo(key));
        Assert.That(result.Properties, Has.Count.EqualTo(2));
        Assert.That(result.Properties["deliveryApi"], Is.EqualTo("Delivery API value"));
        Assert.That(result.Properties["default"], Is.EqualTo("Default value"));
        Assert.That(result.CreateDate, Is.EqualTo(new DateTime(2023, 06, 01)));
        Assert.That(result.UpdateDate, Is.EqualTo(new DateTime(2023, 07, 12)));
    }

    [TestCase("en-US", "2023-08-04")]
    [TestCase("da-DK", "2023-09-08")]
    public void ContentBuilder_MapsContentDatesCorrectlyForCultureVariance(string culture, string expectedUpdateDate)
    {
        var content = new Mock<IPublishedContent>();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        contentType.SetupGet(c => c.Variations).Returns(ContentVariation.Culture);

        var key = Guid.NewGuid();
        var urlSegment = "url-segment";
        var name = "The page";
        ConfigurePublishedContentMock(content, key, name, contentType.Object, []);
        content.SetupGet(c => c.CreateDate).Returns(new DateTime(2023, 07, 02));
        content
            .SetupGet(c => c.Cultures)
            .Returns(new Dictionary<string, PublishedCultureInfo>
            {
                { "en-US", new PublishedCultureInfo("en-US", "EN Name", "en-url-segment", new DateTime(2023, 08, 04)) },
                { "da-DK", new PublishedCultureInfo("da-DK", "DA Name", "da-url-segment", new DateTime(2023, 09, 08)) },
            });

        var routeBuilder = new Mock<IApiContentRouteBuilder>();
        routeBuilder
            .Setup(r => r.Build(content.Object, It.IsAny<string?>()))
            .Returns(new ApiContentRoute(urlSegment, new ApiContentStartItem(Guid.NewGuid(), "/")));

        var variationContextAccessor = new TestVariationContextAccessor { VariationContext = new VariationContext(culture) };

        var builder = new ApiContentBuilder(new ApiContentNameProvider(), routeBuilder.Object, CreateOutputExpansionStrategyAccessor(), variationContextAccessor);
        var result = builder.Build(content.Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.CreateDate, Is.EqualTo(new DateTime(2023, 07, 02)));
        Assert.That(result.UpdateDate, Is.EqualTo(DateTime.Parse(expectedUpdateDate)));
    }

    [Test]
    public void ContentBuilder_CanCustomizeContentNameInDeliveryApiOutput()
    {
        var content = new Mock<IPublishedContent>();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        ConfigurePublishedContentMock(content, Guid.NewGuid(), "The page", contentType.Object, Array.Empty<PublishedPropertyBase>());

        var customNameProvider = new Mock<IApiContentNameProvider>();
        customNameProvider.Setup(n => n.GetName(content.Object)).Returns($"Custom name for: {content.Object.Name}");

        var routeBuilder = new Mock<IApiContentRouteBuilder>();
        routeBuilder
            .Setup(r => r.Build(content.Object, It.IsAny<string?>()))
            .Returns(new ApiContentRoute("the-page", new ApiContentStartItem(Guid.NewGuid(), "/")));

        var builder = new ApiContentBuilder(customNameProvider.Object, routeBuilder.Object, CreateOutputExpansionStrategyAccessor(), CreateVariationContextAccessor());
        var result = builder.Build(content.Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Custom name for: The page"));
    }

    [Test]
    public void ContentBuilder_ReturnsNullForUnRoutableContent()
    {
        var content = new Mock<IPublishedContent>();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        ConfigurePublishedContentMock(content, Guid.NewGuid(), "The page", contentType.Object, Array.Empty<PublishedPropertyBase>());

        var routeBuilder = new Mock<IApiContentRouteBuilder>();
        routeBuilder
            .Setup(r => r.Build(content.Object, It.IsAny<string?>()))
            .Returns((ApiContentRoute)null);

        var builder = new ApiContentBuilder(new ApiContentNameProvider(), routeBuilder.Object, CreateOutputExpansionStrategyAccessor(), CreateVariationContextAccessor());
        var result = builder.Build(content.Object);

        Assert.That(result, Is.Null);
    }
}
