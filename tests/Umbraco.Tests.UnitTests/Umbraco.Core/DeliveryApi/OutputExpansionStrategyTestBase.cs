using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

/// <summary>
/// The tests contained within this class all serve to test property expansion V1 and V2 exactly the same.
/// </summary>
public abstract class OutputExpansionStrategyTestBase : PropertyValueConverterTests
{
    private IPublishedContentType _contentType;
    private IPublishedContentType _elementType;
    private IPublishedContentType _mediaType;

    [SetUp]
    public void SetUp()
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        _contentType = contentType.Object;
        var elementType = new Mock<IPublishedContentType>();
        elementType.SetupGet(c => c.Alias).Returns("theElementType");
        elementType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Element);
        _elementType = elementType.Object;
        var mediaType = new Mock<IPublishedContentType>();
        mediaType.SetupGet(c => c.Alias).Returns("theMediaType");
        mediaType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);
        _mediaType = mediaType.Object;
    }

    [Test]
    public void OutputExpansionStrategy_ExpandsNothingByDefault()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false);
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = new Mock<IPublishedContent>();
        var prop1 = new PublishedElementPropertyBase(DeliveryApiPropertyType, content.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None);

        var contentPickerContent = CreateSimplePickedContent(123, 456);
        var contentPickerProperty = CreateContentPickerProperty(content.Object, contentPickerContent.Key, "contentPicker", apiContentBuilder);

        SetupContentMock(content, prop1, prop2, contentPickerProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(3, result.Properties.Count);
        Assert.AreEqual("Delivery API value", result.Properties[DeliveryApiPropertyType.Alias]);
        Assert.AreEqual("Default value", result.Properties[DefaultPropertyType.Alias]);
        var contentPickerOutput = result.Properties["contentPicker"] as ApiContent;
        Assert.IsNotNull(contentPickerOutput);
        Assert.AreEqual(contentPickerContent.Key, contentPickerOutput.Id);
        Assert.IsEmpty(contentPickerOutput.Properties);
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandSpecificContent()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "contentPickerTwo" });
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = new Mock<IPublishedContent>();

        var contentPickerOneContent = CreateSimplePickedContent(12, 34);
        var contentPickerOneProperty = CreateContentPickerProperty(content.Object, contentPickerOneContent.Key, "contentPickerOne", apiContentBuilder);
        var contentPickerTwoContent = CreateSimplePickedContent(56, 78);
        var contentPickerTwoProperty = CreateContentPickerProperty(content.Object, contentPickerTwoContent.Key, "contentPickerTwo", apiContentBuilder);

        SetupContentMock(content, contentPickerOneProperty, contentPickerTwoProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(2, result.Properties.Count);

        var contentPickerOneOutput = result.Properties["contentPickerOne"] as ApiContent;
        Assert.IsNotNull(contentPickerOneOutput);
        Assert.AreEqual(contentPickerOneContent.Key, contentPickerOneOutput.Id);
        Assert.IsEmpty(contentPickerOneOutput.Properties);

        var contentPickerTwoOutput = result.Properties["contentPickerTwo"] as ApiContent;
        Assert.IsNotNull(contentPickerTwoOutput);
        Assert.AreEqual(contentPickerTwoContent.Key, contentPickerTwoOutput.Id);
        Assert.AreEqual(2, contentPickerTwoOutput.Properties.Count);
        Assert.AreEqual(56, contentPickerTwoOutput.Properties["numberOne"]);
        Assert.AreEqual(78, contentPickerTwoOutput.Properties["numberTwo"]);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OutputExpansionStrategy_CanExpandSpecificMedia(bool mediaPicker3)
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "mediaPickerTwo" });
        var apiMediaBuilder = new ApiMediaBuilder(
            new ApiContentNameProvider(),
            new ApiMediaUrlProvider(PublishedUrlProvider),
            Mock.Of<IPublishedValueFallback>(),
            accessor);

        var media = new Mock<IPublishedContent>();

        var mediaPickerOneContent = CreateSimplePickedMedia(12, 34);
        var mediaPickerOneProperty = mediaPicker3
            ? CreateMediaPicker3Property(media.Object, mediaPickerOneContent.Key, "mediaPickerOne", apiMediaBuilder)
            : CreateMediaPickerProperty(media.Object, mediaPickerOneContent.Key, "mediaPickerOne", apiMediaBuilder);
        var mediaPickerTwoContent = CreateSimplePickedMedia(56, 78);
        var mediaPickerTwoProperty = mediaPicker3
            ? CreateMediaPicker3Property(media.Object, mediaPickerTwoContent.Key, "mediaPickerTwo", apiMediaBuilder)
            : CreateMediaPickerProperty(media.Object, mediaPickerTwoContent.Key, "mediaPickerTwo", apiMediaBuilder);

        SetupMediaMock(media, mediaPickerOneProperty, mediaPickerTwoProperty);

        var result = apiMediaBuilder.Build(media.Object);

        Assert.AreEqual(2, result.Properties.Count);

        var mediaPickerOneOutput = (result.Properties["mediaPickerOne"] as IEnumerable<IApiMedia>)?.FirstOrDefault();
        Assert.IsNotNull(mediaPickerOneOutput);
        Assert.AreEqual(mediaPickerOneContent.Key, mediaPickerOneOutput.Id);
        Assert.IsEmpty(mediaPickerOneOutput.Properties);

        var mediaPickerTwoOutput = (result.Properties["mediaPickerTwo"] as IEnumerable<IApiMedia>)?.FirstOrDefault();
        Assert.IsNotNull(mediaPickerTwoOutput);
        Assert.AreEqual(mediaPickerTwoContent.Key, mediaPickerTwoOutput.Id);
        Assert.AreEqual(2, mediaPickerTwoOutput.Properties.Count);
        Assert.AreEqual(56, mediaPickerTwoOutput.Properties["numberOne"]);
        Assert.AreEqual(78, mediaPickerTwoOutput.Properties["numberTwo"]);
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandAllContent()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(true);
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = new Mock<IPublishedContent>();

        var contentPickerOneContent = CreateSimplePickedContent(12, 34);
        var contentPickerOneProperty = CreateContentPickerProperty(content.Object, contentPickerOneContent.Key, "contentPickerOne", apiContentBuilder);
        var contentPickerTwoContent = CreateSimplePickedContent(56, 78);
        var contentPickerTwoProperty = CreateContentPickerProperty(content.Object, contentPickerTwoContent.Key, "contentPickerTwo", apiContentBuilder);

        SetupContentMock(content, contentPickerOneProperty, contentPickerTwoProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(2, result.Properties.Count);

        var contentPickerOneOutput = result.Properties["contentPickerOne"] as ApiContent;
        Assert.IsNotNull(contentPickerOneOutput);
        Assert.AreEqual(contentPickerOneContent.Key, contentPickerOneOutput.Id);
        Assert.AreEqual(2, contentPickerOneOutput.Properties.Count);
        Assert.AreEqual(12, contentPickerOneOutput.Properties["numberOne"]);
        Assert.AreEqual(34, contentPickerOneOutput.Properties["numberTwo"]);

        var contentPickerTwoOutput = result.Properties["contentPickerTwo"] as ApiContent;
        Assert.IsNotNull(contentPickerTwoOutput);
        Assert.AreEqual(contentPickerTwoContent.Key, contentPickerTwoOutput.Id);
        Assert.AreEqual(2, contentPickerTwoOutput.Properties.Count);
        Assert.AreEqual(56, contentPickerTwoOutput.Properties["numberOne"]);
        Assert.AreEqual(78, contentPickerTwoOutput.Properties["numberTwo"]);
    }

    [TestCase("contentPicker", "contentPicker")]
    [TestCase("rootPicker", "nestedPicker")]
    public void OutputExpansionStrategy_DoesNotExpandNestedContentPicker(string rootPropertyTypeAlias, string nestedPropertyTypeAlias)
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { rootPropertyTypeAlias, nestedPropertyTypeAlias });
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = new Mock<IPublishedContent>();

        var nestedContentPickerContent = CreateSimplePickedContent(987, 654);
        var contentPickerContent = CreateMultiLevelPickedContent(123, nestedContentPickerContent, nestedPropertyTypeAlias, apiContentBuilder);
        var contentPickerContentProperty = CreateContentPickerProperty(content.Object, contentPickerContent.Key, rootPropertyTypeAlias, apiContentBuilder);

        SetupContentMock(content, contentPickerContentProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(1, result.Properties.Count);

        var contentPickerOneOutput = result.Properties[rootPropertyTypeAlias] as ApiContent;
        Assert.IsNotNull(contentPickerOneOutput);
        Assert.AreEqual(contentPickerContent.Key, contentPickerOneOutput.Id);
        Assert.AreEqual(2, contentPickerOneOutput.Properties.Count);
        Assert.AreEqual(123, contentPickerOneOutput.Properties["number"]);

        var nestedContentPickerOutput = contentPickerOneOutput.Properties[nestedPropertyTypeAlias] as ApiContent;
        Assert.IsNotNull(nestedContentPickerOutput);
        Assert.AreEqual(nestedContentPickerContent.Key, nestedContentPickerOutput.Id);
        Assert.IsEmpty(nestedContentPickerOutput.Properties);
    }

    [Test]
    public void OutputExpansionStrategy_DoesNotExpandElementsByDefault()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false);
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var contentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPicker2Value = CreateSimplePickedContent(666, 777);

        var content = new Mock<IPublishedContent>();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, 444, "number"),
            CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder),
            CreateElementProperty(content.Object, "element2", 555, contentPicker2Value.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(3, result.Properties.Count);
        Assert.AreEqual(444, result.Properties["number"]);

        var expectedElementOutputs = new[]
        {
            new
            {
                PropertyAlias = "element",
                ElementNumber = 333,
                ElementContentPicker = contentPickerValue.Key
            },
            new
            {
                PropertyAlias = "element2",
                ElementNumber = 555,
                ElementContentPicker = contentPicker2Value.Key
            }
        };

        foreach (var expectedElementOutput in expectedElementOutputs)
        {
            var elementOutput = result.Properties[expectedElementOutput.PropertyAlias] as IApiElement;
            Assert.IsNotNull(elementOutput);
            Assert.AreEqual(2, elementOutput.Properties.Count);
            Assert.AreEqual(expectedElementOutput.ElementNumber, elementOutput.Properties["number"]);
            var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
            Assert.IsNotNull(contentPickerOutput);
            Assert.AreEqual(expectedElementOutput.ElementContentPicker, contentPickerOutput.Id);
            Assert.AreEqual(0, contentPickerOutput.Properties.Count);
        }
    }

    [Test]
    public void OutputExpansionStrategy_MappingContent_ThrowsOnInvalidItemType()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false);
        if (accessor.TryGetValue(out IOutputExpansionStrategy outputExpansionStrategy) is false)
        {
            Assert.Fail("Could not obtain the output expansion strategy");
        }

        Assert.Throws<ArgumentException>(() => outputExpansionStrategy.MapContentProperties(PublishedMedia));
    }

    [Test]
    public void OutputExpansionStrategy_MappingMedia_ThrowsOnInvalidItemType()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false);
        if (accessor.TryGetValue(out IOutputExpansionStrategy outputExpansionStrategy) is false)
        {
            Assert.Fail("Could not obtain the output expansion strategy");
        }

        Assert.Throws<ArgumentException>(() => outputExpansionStrategy.MapMediaProperties(PublishedContent));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void OutputExpansionStrategy_ForwardsExpansionStateToPropertyValueConverter(bool expanding)
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { expanding ? "theAlias" : "noSuchAlias" });
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = new Mock<IPublishedContent>();

        var valueConverterMock = new Mock<IDeliveryApiPropertyValueConverter>();
        valueConverterMock.Setup(v => v.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        valueConverterMock.Setup(p => p.IsValue(It.IsAny<object?>(), It.IsAny<PropertyValueLevel>())).Returns(true);
        valueConverterMock.Setup(v => v.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        valueConverterMock.Setup(v => v.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        valueConverterMock.Setup(v => v.GetDeliveryApiPropertyCacheLevelForExpansion(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        valueConverterMock.Setup(v => v.ConvertIntermediateToDeliveryApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(expanding ? "Expanding" : "Not expanding");

        var propertyType = SetupPublishedPropertyType(valueConverterMock.Object, "theAlias", Constants.PropertyEditors.Aliases.Label);
        var property = new PublishedElementPropertyBase(propertyType, content.Object, false, PropertyCacheLevel.None, "The Value");

        SetupContentMock(content, property);

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(1, result.Properties.Count);
        Assert.AreEqual(expanding ? "Expanding" : "Not expanding", result.Properties["theAlias"] as string);
    }

    protected abstract IOutputExpansionStrategyAccessor CreateOutputExpansionStrategyAccessor(string? expand = null, string? fields = null);

    protected IOutputExpansionStrategyAccessor CreateOutputExpansionStrategyAccessor(bool expandAll = false, string[]? expandPropertyAliases = null)
        => CreateOutputExpansionStrategyAccessor(FormatExpandSyntax(expandAll, expandPropertyAliases));

    protected abstract string? FormatExpandSyntax(bool expandAll = false, string[]? expandPropertyAliases = null);

    protected void SetupContentMock(Mock<IPublishedContent> content, params IPublishedProperty[] properties)
    {
        var key = Guid.NewGuid();
        var name = "The page";
        var urlSegment = "url-segment";
        ConfigurePublishedContentMock(content, key, name, urlSegment, _contentType, properties);

        RegisterContentWithProviders(content.Object);
    }

    protected void SetupMediaMock(Mock<IPublishedContent> media, params IPublishedProperty[] properties)
    {
        var key = Guid.NewGuid();
        var name = "The media";
        var urlSegment = "media-url-segment";
        ConfigurePublishedContentMock(media, key, name, urlSegment, _mediaType, properties);

        RegisterMediaWithProviders(media.Object);
    }

    protected IPublishedContent CreateSimplePickedContent(int numberOneValue, int numberTwoValue)
    {
        var content = new Mock<IPublishedContent>();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, numberOneValue, "numberOne"),
            CreateNumberProperty(content.Object, numberTwoValue, "numberTwo"));

        return content.Object;
    }

    protected IPublishedContent CreateSimplePickedMedia(int numberOneValue, int numberTwoValue)
    {
        var media = new Mock<IPublishedContent>();
        SetupMediaMock(
            media,
            CreateNumberProperty(media.Object, numberOneValue, "numberOne"),
            CreateNumberProperty(media.Object, numberTwoValue, "numberTwo"));

        return media.Object;
    }

    protected IPublishedContent CreateMultiLevelPickedContent(int numberValue, IPublishedContent nestedContentPickerValue, string nestedContentPickerPropertyTypeAlias, ApiContentBuilder apiContentBuilder)
    {
        var content = new Mock<IPublishedContent>();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, numberValue, "number"),
            CreateContentPickerProperty(content.Object, nestedContentPickerValue.Key, nestedContentPickerPropertyTypeAlias, apiContentBuilder));

        return content.Object;
    }

    internal PublishedElementPropertyBase CreateContentPickerProperty(IPublishedElement parent, Guid pickedContentKey, string propertyTypeAlias, IApiContentBuilder contentBuilder)
    {
        ContentPickerValueConverter contentPickerValueConverter = new ContentPickerValueConverter(PublishedSnapshotAccessor, contentBuilder);
        var contentPickerPropertyType = SetupPublishedPropertyType(contentPickerValueConverter, propertyTypeAlias, Constants.PropertyEditors.Aliases.ContentPicker);

        return new PublishedElementPropertyBase(contentPickerPropertyType, parent, false, PropertyCacheLevel.None, new GuidUdi(Constants.UdiEntityType.Document, pickedContentKey).ToString());
    }

    internal PublishedElementPropertyBase CreateMediaPickerProperty(IPublishedElement parent, Guid pickedMediaKey, string propertyTypeAlias, IApiMediaBuilder mediaBuilder)
    {
        MediaPickerValueConverter mediaPickerValueConverter = new MediaPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IPublishedModelFactory>(), mediaBuilder);
        var mediaPickerPropertyType = SetupPublishedPropertyType(mediaPickerValueConverter, propertyTypeAlias, Constants.PropertyEditors.Aliases.MediaPicker, new MediaPickerConfiguration());

        return new PublishedElementPropertyBase(mediaPickerPropertyType, parent, false, PropertyCacheLevel.None, new GuidUdi(Constants.UdiEntityType.Media, pickedMediaKey).ToString());
    }

    internal PublishedElementPropertyBase CreateMediaPicker3Property(IPublishedElement parent, Guid pickedMediaKey, string propertyTypeAlias, IApiMediaBuilder mediaBuilder)
    {
        var serializer = new JsonNetSerializer();
        var value = serializer.Serialize(new[]
        {
            new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
            {
                MediaKey = pickedMediaKey
            }
        });

        var publishedValueFallback = Mock.Of<IPublishedValueFallback>();
        var apiMediaWithCropsBuilder = new ApiMediaWithCropsBuilder(mediaBuilder, publishedValueFallback);

        MediaPickerWithCropsValueConverter mediaPickerValueConverter = new MediaPickerWithCropsValueConverter(PublishedSnapshotAccessor, PublishedUrlProvider, publishedValueFallback, new JsonNetSerializer(), apiMediaWithCropsBuilder);
        var mediaPickerPropertyType = SetupPublishedPropertyType(mediaPickerValueConverter, propertyTypeAlias, Constants.PropertyEditors.Aliases.MediaPicker3, new MediaPicker3Configuration());

        return new PublishedElementPropertyBase(mediaPickerPropertyType, parent, false, PropertyCacheLevel.None, value);
    }

    internal PublishedElementPropertyBase CreateNumberProperty(IPublishedElement parent, int propertyValue, string propertyTypeAlias)
    {
        var numberPropertyType = SetupPublishedPropertyType(new IntegerValueConverter(), propertyTypeAlias, Constants.PropertyEditors.Aliases.Label);
        return new PublishedElementPropertyBase(numberPropertyType, parent, false, PropertyCacheLevel.None, propertyValue);
    }

    internal PublishedElementPropertyBase CreateElementProperty(
        IPublishedElement parent,
        string elementPropertyAlias,
        int numberPropertyValue,
        Guid contentPickerPropertyValue,
        string contentPickerPropertyTypeAlias,
        IApiContentBuilder apiContentBuilder,
        IApiElementBuilder apiElementBuilder)
    {
        var element = new Mock<IPublishedElement>();
        element.SetupGet(c => c.ContentType).Returns(_elementType);
        element.SetupGet(c => c.Properties).Returns(new[]
        {
            CreateNumberProperty(element.Object, numberPropertyValue, "number"),
            CreateContentPickerProperty(element.Object, contentPickerPropertyValue, contentPickerPropertyTypeAlias, apiContentBuilder)
        });

        var elementValueConverter = new Mock<IDeliveryApiPropertyValueConverter>();
        elementValueConverter
            .Setup(p => p.ConvertIntermediateToDeliveryApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(() => apiElementBuilder.Build(element.Object));
        elementValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        elementValueConverter.Setup(p => p.IsValue(It.IsAny<object?>(), It.IsAny<PropertyValueLevel>())).Returns(true);
        elementValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        elementValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        elementValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevelForExpansion(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);

        var elementPropertyType = SetupPublishedPropertyType(elementValueConverter.Object, elementPropertyAlias, "My.Element.Property");
        return new PublishedElementPropertyBase(elementPropertyType, parent, false, PropertyCacheLevel.None);
    }

    protected IApiContentRouteBuilder ApiContentRouteBuilder() => CreateContentRouteBuilder(PublishedUrlProvider, CreateGlobalSettings());
}
