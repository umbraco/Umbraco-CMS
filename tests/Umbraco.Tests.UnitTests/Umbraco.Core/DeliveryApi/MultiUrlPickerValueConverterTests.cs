using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MultiUrlPickerValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void MultiUrlPickerValueConverter_InSingleMode_ConvertsContentToLinksWithContentInfo()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<ApiLink>)));

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var link = result.First();
        Assert.That(link.Title, Is.EqualTo(PublishedContent.Name));
        Assert.That(link.LinkType, Is.EqualTo(LinkType.Content));
        Assert.That(link.DestinationId, Is.EqualTo(PublishedContent.Key));
        Assert.That(link.DestinationType, Is.EqualTo("TheContentType"));
        Assert.That(link.Url, Is.Null);
        Assert.That(link.Target, Is.Null);
        var route = link.Route;
        Assert.That(route, Is.Not.Null);
        Assert.That(route.Path, Is.EqualTo("/the-page-url/"));
    }

    [Test]
    public void MultiUrlPickerValueConverter_InSingleMode_ConvertsMediaToLinksWithoutContentInfo()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<ApiLink>)));

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var link = result.First();
        Assert.That(link.Title, Is.EqualTo(PublishedMedia.Name));
        Assert.That(link.DestinationId, Is.EqualTo(PublishedMedia.Key));
        Assert.That(link.DestinationType, Is.EqualTo("TheMediaType"));
        Assert.That(link.Url, Is.EqualTo("the-media-url"));
        Assert.That(link.LinkType, Is.EqualTo(LinkType.Media));
        Assert.That(link.Target, Is.EqualTo(null));
        Assert.That(link.Route, Is.EqualTo(null));
    }

    [Test]
    public void MultiUrlPickerValueConverter_InMultiMode_CanHandleMixedLinkTypes()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 10 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<ApiLink>)));

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)
            },
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)
            },
            new MultiUrlPickerValueEditor.LinkDto
            {
                Name = "The link",
                QueryString = "?something=true",
                Target = "_blank",
                Url = "https://umbraco.com/"
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(3));

        var first = result.First();
        var second = result.Skip(1).First();
        var last = result.Last();

        Assert.That(first.Title, Is.EqualTo(PublishedContent.Name));
        Assert.That(first.LinkType, Is.EqualTo(LinkType.Content));
        Assert.That(first.DestinationId, Is.EqualTo(PublishedContent.Key));
        Assert.That(first.Route, Is.Not.Null);

        Assert.That(second.Title, Is.EqualTo(PublishedMedia.Name));
        Assert.That(second.DestinationId, Is.EqualTo(PublishedMedia.Key));
        Assert.That(second.DestinationType, Is.EqualTo("TheMediaType"));
        Assert.That(second.Route, Is.Null);

        Assert.That(last.Title, Is.EqualTo("The link"));
        Assert.That(last.Url, Is.EqualTo("https://umbraco.com/?something=true"));
        Assert.That(last.LinkType, Is.EqualTo(LinkType.External));
        Assert.That(last.Target, Is.EqualTo("_blank"));
        Assert.That(last.Route, Is.Null);
    }

    [Test]
    public void MultiUrlPickerValueConverter_ConvertsExternalUrlToLinks()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Name = "The link",
                QueryString = "?something=true",
                Target = "_blank",
                Url = "https://umbraco.com/"
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var link = result.First();
        Assert.That(link.Title, Is.EqualTo("The link"));
        Assert.That(link.Url, Is.EqualTo("https://umbraco.com/?something=true"));
        Assert.That(link.QueryString, Is.EqualTo("?something=true"));
        Assert.That(link.LinkType, Is.EqualTo(LinkType.External));
        Assert.That(link.Target, Is.EqualTo("_blank"));
        Assert.That(link.Route, Is.Null);
    }

    [Test]
    public void MultiUrlPickerValueConverter_AppliesExplicitConfigurationToMediaLink()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key),
                Name = "Custom link name",
                QueryString = "?something=true",
                Target = "_blank"
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var link = result.First();
        Assert.That(link.Title, Is.EqualTo("Custom link name"));
        Assert.That(link.DestinationId, Is.EqualTo(PublishedMedia.Key));
        Assert.That(link.DestinationType, Is.EqualTo("TheMediaType"));
        Assert.That(link.Url, Is.EqualTo("the-media-url?something=true"));
        Assert.That(link.LinkType, Is.EqualTo(LinkType.Media));
        Assert.That(link.Target, Is.EqualTo("_blank"));
        Assert.That(link.QueryString, Is.EqualTo("?something=true"));
        Assert.That(link.Route, Is.EqualTo(null));
    }

    [Test]
    public void MultiUrlPickerValueConverter_AppliesExplicitConfigurationToContentLink()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
                Name = "Custom link name",
                QueryString = "?something=true",
                Target = "_blank"
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var link = result.First();
        Assert.That(link.Title, Is.EqualTo("Custom link name"));
        Assert.That(link.DestinationId, Is.EqualTo(PublishedContent.Key));
        Assert.That(link.Route!.Path, Is.EqualTo("/the-page-url/"));
        Assert.That(link.LinkType, Is.EqualTo(LinkType.Content));
        Assert.That(link.Target, Is.EqualTo("_blank"));
        Assert.That(link.QueryString, Is.EqualTo("?something=true"));
        Assert.That(link.Url, Is.Null);
    }

    [Test]
    public void MultiUrlPickerValueConverter_PrioritizesContentUrlOverConfiguredUrl()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
                Url = "https://umbraco.com/",
                QueryString = "?something=true"
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var link = result.First();
        Assert.That(link.Title, Is.EqualTo(PublishedContent.Name));
        Assert.That(link.DestinationId, Is.EqualTo(PublishedContent.Key));
        Assert.That(link.Route!.Path, Is.EqualTo("/the-page-url/"));
        Assert.That(link.LinkType, Is.EqualTo(LinkType.Content));
        Assert.That(link.Target, Is.Null);
        Assert.That(link.Url, Is.Null);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiUrlPickerValueConverter_InSingleMode_ConvertsInvalidValueToEmptyArray(object? inter)
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiUrlPickerValueConverter_InMultiMode_ConvertsInvalidValueToEmptyArray(object? inter)
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 10 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    private IApiMediaUrlProvider ApiMediaUrlProvider() => new ApiMediaUrlProvider(PublishedUrlProvider);

    private MultiUrlPickerValueConverter MultiUrlPickerValueConverter()
    {
        var routeBuilder = CreateContentRouteBuilder(ApiContentPathProvider, CreateGlobalSettings());
        return new MultiUrlPickerValueConverter(
            Mock.Of<IProfilingLogger>(),
            Serializer(),
            PublishedUrlProvider,
            new ApiContentNameProvider(),
            ApiMediaUrlProvider(),
            routeBuilder,
            CacheManager.Content,
            CacheManager.Media);
    }

    private IJsonSerializer Serializer() => new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [Test]
    public void MultiUrlPickerValueConverter_DeliveryApi_ContentLinkWithCulture_IncludesCultureInApiLink()
    {
        // Arrange
        var publishedDataType = new PublishedDataType(
            123, "test", "test",
            new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        // Setup culture-specific URL path
        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(
                PublishedContent,
                It.IsAny<UrlMode>(),
                "fr-FR",
                It.IsAny<Uri?>()))
            .Returns("/fr/the-page-url");

        var valueConverter = MultiUrlPickerValueConverter();

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
                Culture = "fr-FR"
            }
        });

        // Act
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedElement>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            inter,
            false,
            false) as IEnumerable<ApiLink>;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var link = result.First();
        Assert.That(link.Title, Is.EqualTo(PublishedContent.Name));
        Assert.That(link.LinkType, Is.EqualTo(LinkType.Content));
        Assert.That(link.DestinationId, Is.EqualTo(PublishedContent.Key));
        Assert.That(link.DestinationType, Is.EqualTo("TheContentType"));
        Assert.That(link.Url, Is.Null);
        Assert.That(link.Target, Is.Null);
        Assert.That(link.Culture, Is.EqualTo("fr-FR"));
        var route = link.Route;
        Assert.That(route, Is.Not.Null);
        Assert.That(route.Path, Is.EqualTo("/fr/the-page-url/"));
    }

    [Test]
    public void MultiUrlPickerValueConverter_ConvertToObject_ContentLinkWithCulture_GeneratesUrlWithCorrectCulture()
    {
        // Arrange
        var publishedDataType = new PublishedDataType(
            123, "test", "test",
            new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);
        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(
                PublishedContent,
                It.IsAny<UrlMode>(),
                "fr-FR",
                It.IsAny<Uri?>()))
            .Returns("/fr/the-page-url");
        var valueConverter = MultiUrlPickerValueConverter();
        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
                Culture = "fr-FR"
            }
        });

        // Act
        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            inter,
            false);

        // Assert
        Assert.That(result, Is.Not.Null);
        var link = result as Link;
        Assert.That(link.Url, Is.EqualTo("/fr/the-page-url"));
    }
}
