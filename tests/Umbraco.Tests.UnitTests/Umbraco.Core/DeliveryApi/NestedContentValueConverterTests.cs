using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class NestedContentValueConverterTests : PropertyValueConverterTests
{
    private IPublishedModelFactory _publishedModelFactory;
    private IApiElementBuilder _apiElementBuilder;
    private NestedContentSingleValueConverter _nestedContentSingleValueConverter;
    private NestedContentManyValueConverter _nestedContentManyValueConverter;
    private IPublishedPropertyType _publishedPropertyType;

    [SetUp]
    public void SetupThis()
    {
        var publishedModelFactoryMock = new Mock<IPublishedModelFactory>();
        publishedModelFactoryMock
            .Setup(m => m.CreateModel(It.IsAny<IPublishedElement>()))
            .Returns((IPublishedElement element) => element);
        _publishedModelFactory = publishedModelFactoryMock.Object;

        _apiElementBuilder = new ApiElementBuilder(CreateOutputExpansionStrategyAccessor());

        var profilingLogger = new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());

        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new NestedContentConfiguration { MaxItems = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);
        publishedPropertyType.SetupGet(p => p.Alias).Returns("prop1");
        publishedPropertyType.SetupGet(p => p.CacheLevel).Returns(PropertyCacheLevel.Element);
        publishedPropertyType.SetupGet(p => p.DeliveryApiCacheLevel).Returns(PropertyCacheLevel.Element);
        publishedPropertyType
            .Setup(p => p.ConvertSourceToInter(It.IsAny<IPublishedElement>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns((IPublishedElement owner, object? source, bool preview) => source);
        publishedPropertyType
            .Setup(p => p.ConvertInterToDeliveryApiObject(It.IsAny<IPublishedElement>(), PropertyCacheLevel.Element, It.IsAny<object>(), It.IsAny<bool>()))
            .Returns((IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview) => inter?.ToString());
        _publishedPropertyType = publishedPropertyType.Object;

        var publishedContentType = new Mock<IPublishedContentType>();
        publishedContentType.SetupGet(c => c.IsElement).Returns(true);
        publishedContentType.SetupGet(c => c.Alias).Returns("contentType1");
        publishedContentType.SetupGet(c => c.PropertyTypes).Returns(new[] { publishedPropertyType.Object });

        PublishedContentCacheMock
            .Setup(m => m.GetContentType("contentType1"))
            .Returns(publishedContentType.Object);

        _nestedContentSingleValueConverter = new NestedContentSingleValueConverter(PublishedSnapshotAccessor, _publishedModelFactory, profilingLogger, _apiElementBuilder);
        _nestedContentManyValueConverter = new NestedContentManyValueConverter(PublishedSnapshotAccessor, _publishedModelFactory, profilingLogger, _apiElementBuilder);
    }

    [Test]
    public void NestedContentSingleValueConverter_HasMultipleElementsAsDeliveryApiType()
        => Assert.AreEqual(typeof(IEnumerable<IApiElement>), _nestedContentSingleValueConverter.GetDeliveryApiPropertyValueType(Mock.Of<IPublishedPropertyType>()));

    [Test]
    public void NestedContentSingleValueConverter_WithOneItem_ConvertsItemToListOfElements()
    {
        var nestedContentValue = "[{\"ncContentTypeAlias\": \"contentType1\",\"key\": \"1E68FB92-727A-4473-B10C-FA108ADCF16F\",\"prop1\": \"Hello, world\"}]";
        var result = _nestedContentSingleValueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), _publishedPropertyType, PropertyCacheLevel.Element, nestedContentValue, false) as IEnumerable<IApiElement>;

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("contentType1", result.First().ContentType);
        Assert.AreEqual(Guid.Parse("1E68FB92-727A-4473-B10C-FA108ADCF16F"), result.First().Id);
        Assert.AreEqual(1, result.First().Properties.Count);
        Assert.AreEqual("Hello, world", result.First().Properties["prop1"]);
    }

    [Test]
    public void NestedContentSingleValueConverter_WithMultipleItems_ConvertsFirstItemToListOfElements()
    {
        var nestedContentValue = "[{\"ncContentTypeAlias\": \"contentType1\",\"key\": \"1E68FB92-727A-4473-B10C-FA108ADCF16F\",\"prop1\": \"Hello, world\"},{\"ncContentTypeAlias\": \"contentType1\",\"key\": \"40F59DD9-7E9F-4053-BD32-89FB086D18C9\",\"prop1\": \"One more\"}]";
        var result = _nestedContentSingleValueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), _publishedPropertyType, PropertyCacheLevel.Element, nestedContentValue, false) as IEnumerable<IApiElement>;

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("contentType1", result.First().ContentType);
        Assert.AreEqual(Guid.Parse("1E68FB92-727A-4473-B10C-FA108ADCF16F"), result.First().Id);
        Assert.AreEqual(1, result.First().Properties.Count);
        Assert.AreEqual("Hello, world", result.First().Properties["prop1"]);
    }

    [Test]
    public void NestedContentSingleValueConverter_WithNoData_ReturnsEmptyArray()
    {
        var result = _nestedContentSingleValueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), _publishedPropertyType, PropertyCacheLevel.Element, null, false) as IEnumerable<IApiElement>;

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void NestedContentManyValueConverter_HasMultipleElementsAsDeliveryApiType()
        => Assert.AreEqual(typeof(IEnumerable<IApiElement>), _nestedContentManyValueConverter.GetDeliveryApiPropertyValueType(Mock.Of<IPublishedPropertyType>()));


    [Test]
    public void NestedContentManyValueConverter_WithOneItem_ConvertsItemToListOfElements()
    {
        var nestedContentValue = "[{\"ncContentTypeAlias\": \"contentType1\",\"key\": \"1E68FB92-727A-4473-B10C-FA108ADCF16F\",\"prop1\": \"Hello, world\"}]";
        var result = _nestedContentManyValueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), _publishedPropertyType, PropertyCacheLevel.Element, nestedContentValue, false) as IEnumerable<IApiElement>;

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());

        Assert.AreEqual("contentType1", result.First().ContentType);
        Assert.AreEqual(Guid.Parse("1E68FB92-727A-4473-B10C-FA108ADCF16F"), result.First().Id);
        Assert.AreEqual(1, result.First().Properties.Count);
        Assert.AreEqual("Hello, world", result.First().Properties["prop1"]);
    }

    [Test]
    public void NestedContentManyValueConverter_WithMultipleItems_ConvertsAllItemsToElements()
    {
        var nestedContentValue = "[{\"ncContentTypeAlias\": \"contentType1\",\"key\": \"1E68FB92-727A-4473-B10C-FA108ADCF16F\",\"prop1\": \"Hello, world\"},{\"ncContentTypeAlias\": \"contentType1\",\"key\": \"40F59DD9-7E9F-4053-BD32-89FB086D18C9\",\"prop1\": \"One more\"}]";
        var result = _nestedContentManyValueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), _publishedPropertyType, PropertyCacheLevel.Element, nestedContentValue, false) as IEnumerable<IApiElement>;

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual("contentType1", result.First().ContentType);
        Assert.AreEqual(Guid.Parse("1E68FB92-727A-4473-B10C-FA108ADCF16F"), result.First().Id);
        Assert.AreEqual(1, result.First().Properties.Count);
        Assert.AreEqual("Hello, world", result.First().Properties["prop1"]);

        Assert.AreEqual("contentType1", result.Last().ContentType);
        Assert.AreEqual(Guid.Parse("40F59DD9-7E9F-4053-BD32-89FB086D18C9"), result.Last().Id);
        Assert.AreEqual(1, result.Last().Properties.Count);
        Assert.AreEqual("One more", result.Last().Properties["prop1"]);
    }

    [Test]
    public void NestedContentManyValueConverter_WithNoData_ReturnsEmptyArray()
    {
        var result = _nestedContentManyValueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), _publishedPropertyType, PropertyCacheLevel.Element, null, false) as IEnumerable<IApiElement>;

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }
}
