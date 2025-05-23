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
        Assert.AreEqual(typeof(IEnumerable<ApiLink>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        var link = result.First();
        Assert.AreEqual(PublishedContent.Name, link.Title);
        Assert.AreEqual(LinkType.Content, link.LinkType);
        Assert.AreEqual(PublishedContent.Key, link.DestinationId);
        Assert.AreEqual("TheContentType", link.DestinationType);
        Assert.Null(link.Url);
        Assert.Null(link.Target);
        var route = link.Route;
        Assert.NotNull(route);
        Assert.AreEqual("/the-page-url/", route.Path);
    }

    [Test]
    public void MultiUrlPickerValueConverter_InSingleMode_ConvertsMediaToLinksWithoutContentInfo()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();
        Assert.AreEqual(typeof(IEnumerable<ApiLink>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = Serializer().Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)
            }
        });
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<ApiLink>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        var link = result.First();
        Assert.AreEqual(PublishedMedia.Name, link.Title);
        Assert.AreEqual(PublishedMedia.Key, link.DestinationId);
        Assert.AreEqual("TheMediaType", link.DestinationType);
        Assert.AreEqual("the-media-url", link.Url);
        Assert.AreEqual(LinkType.Media, link.LinkType);
        Assert.AreEqual(null, link.Target);
        Assert.AreEqual(null, link.Route);
    }

    [Test]
    public void MultiUrlPickerValueConverter_InMultiMode_CanHandleMixedLinkTypes()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 10 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiUrlPickerValueConverter();
        Assert.AreEqual(typeof(IEnumerable<ApiLink>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

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
        Assert.NotNull(result);
        Assert.AreEqual(3, result.Count());

        var first = result.First();
        var second = result.Skip(1).First();
        var last = result.Last();

        Assert.AreEqual(PublishedContent.Name, first.Title);
        Assert.AreEqual(LinkType.Content, first.LinkType);
        Assert.AreEqual(PublishedContent.Key, first.DestinationId);
        Assert.NotNull(first.Route);

        Assert.AreEqual(PublishedMedia.Name, second.Title);
        Assert.AreEqual(PublishedMedia.Key, second.DestinationId);
        Assert.AreEqual("TheMediaType", second.DestinationType);
        Assert.Null(second.Route);

        Assert.AreEqual("The link", last.Title);
        Assert.AreEqual("https://umbraco.com/?something=true", last.Url);
        Assert.AreEqual(LinkType.External, last.LinkType);
        Assert.AreEqual("_blank", last.Target);
        Assert.Null(last.Route);
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
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        var link = result.First();
        Assert.AreEqual("The link", link.Title);
        Assert.AreEqual("https://umbraco.com/?something=true", link.Url);
        Assert.AreEqual("?something=true", link.QueryString);
        Assert.AreEqual(LinkType.External, link.LinkType);
        Assert.AreEqual("_blank", link.Target);
        Assert.Null(link.Route);
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
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        var link = result.First();
        Assert.AreEqual("Custom link name", link.Title);
        Assert.AreEqual(PublishedMedia.Key, link.DestinationId);
        Assert.AreEqual("TheMediaType", link.DestinationType);
        Assert.AreEqual("the-media-url?something=true", link.Url);
        Assert.AreEqual(LinkType.Media, link.LinkType);
        Assert.AreEqual("_blank", link.Target);
        Assert.AreEqual("?something=true", link.QueryString);
        Assert.AreEqual(null, link.Route);
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
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        var link = result.First();
        Assert.AreEqual("Custom link name", link.Title);
        Assert.AreEqual(PublishedContent.Key, link.DestinationId);
        Assert.AreEqual("/the-page-url/",  link.Route!.Path);
        Assert.AreEqual(LinkType.Content, link.LinkType);
        Assert.AreEqual("_blank", link.Target);
        Assert.AreEqual("?something=true", link.QueryString);
        Assert.Null(link.Url);
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
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        var link = result.First();
        Assert.AreEqual(PublishedContent.Name, link.Title);
        Assert.AreEqual(PublishedContent.Key, link.DestinationId);
        Assert.AreEqual("/the-page-url/", link.Route!.Path);
        Assert.AreEqual(LinkType.Content, link.LinkType);
        Assert.Null(link.Target);
        Assert.Null(link.Url);
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
        Assert.NotNull(result);
        Assert.IsEmpty(result);
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
        Assert.NotNull(result);
        Assert.IsEmpty(result);
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

    private IJsonSerializer Serializer() => new SystemTextJsonSerializer();
}
