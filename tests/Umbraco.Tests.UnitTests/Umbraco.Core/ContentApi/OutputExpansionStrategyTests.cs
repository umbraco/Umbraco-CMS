using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Content.Rendering;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ContentApi;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class OutputExpansionStrategyTests : PropertyValueConverterTests
{
    private IPublishedContentType _contentType;
    private IPublishedContentType _elementType;

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
    }

    [Test]
    public void OutputExpansionStrategy_ExpandsNothingByDefault()
    {
        var accessor = CreateOutputExpansionStrategyAccessor();
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = new Mock<IPublishedContent>();
        var prop1 = new PublishedElementPropertyBase(ContentApiPropertyType, content.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None);

        var contentPickerContent = CreateSimplePickedContent(123, 456);
        var contentPickerProperty = CreateContentPickerProperty(content.Object, contentPickerContent.Key, "contentPicker", apiContentBuilder);

        SetupContentMock(content, prop1, prop2, contentPickerProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(3, result.Properties.Count);
        Assert.AreEqual("Content API value", result.Properties[ContentApiPropertyType.Alias]);
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
        var accessor = CreateOutputExpansionStrategyAccessor();
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
    public void OutputExpansionStrategy_CanExpandSpecifiedElement()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "element" });
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

        var elementOutput = result.Properties["element"] as IApiElement;
        Assert.IsNotNull(elementOutput);
        Assert.AreEqual(2, elementOutput.Properties.Count);
        Assert.AreEqual(333, elementOutput.Properties["number"]);
        var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(contentPickerOutput);
        Assert.AreEqual(contentPickerValue.Key, contentPickerOutput.Id);
        Assert.AreEqual(2, contentPickerOutput.Properties.Count);
        Assert.AreEqual(111, contentPickerOutput.Properties["numberOne"]);
        Assert.AreEqual(222, contentPickerOutput.Properties["numberTwo"]);

        elementOutput = result.Properties["element2"] as IApiElement;
        Assert.IsNotNull(elementOutput);
        Assert.AreEqual(2, elementOutput.Properties.Count);
        Assert.AreEqual(555, elementOutput.Properties["number"]);
        contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(contentPickerOutput);
        Assert.AreEqual(contentPicker2Value.Key, contentPickerOutput.Id);
        Assert.AreEqual(0, contentPickerOutput.Properties.Count);
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandAllElements()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(true );
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
                ElementContentPicker = contentPickerValue.Key,
                ContentNumberOne = 111,
                ContentNumberTwo = 222
            },
            new
            {
                PropertyAlias = "element2",
                ElementNumber = 555,
                ElementContentPicker = contentPicker2Value.Key,
                ContentNumberOne = 666,
                ContentNumberTwo = 777
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
            Assert.AreEqual(2, contentPickerOutput.Properties.Count);
            Assert.AreEqual(expectedElementOutput.ContentNumberOne, contentPickerOutput.Properties["numberOne"]);
            Assert.AreEqual(expectedElementOutput.ContentNumberTwo, contentPickerOutput.Properties["numberTwo"]);
        }
    }

    [Test]
    public void OutputExpansionStrategy_DoesNotExpandElementNestedContentPicker()
    {
        var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "element" });
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var nestedContentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPickerValue = CreateMultiLevelPickedContent(987, nestedContentPickerValue, "contentPicker", apiContentBuilder);

        var content = new Mock<IPublishedContent>();
        SetupContentMock(content, CreateElementProperty(content.Object, "element", 333, contentPickerValue.Key, "contentPicker", apiContentBuilder, apiElementBuilder));

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(1, result.Properties.Count);

        var elementOutput = result.Properties["element"] as IApiElement;
        Assert.IsNotNull(elementOutput);
        Assert.AreEqual(2, elementOutput.Properties.Count);
        Assert.AreEqual(333, elementOutput.Properties["number"]);
        var contentPickerOutput = elementOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(contentPickerOutput);
        Assert.AreEqual(contentPickerValue.Key, contentPickerOutput.Id);
        Assert.AreEqual(2, contentPickerOutput.Properties.Count);
        Assert.AreEqual(987, contentPickerOutput.Properties["number"]);
        var nestedContentPickerOutput = contentPickerOutput.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(nestedContentPickerOutput);
        Assert.AreEqual(nestedContentPickerValue.Key, nestedContentPickerOutput.Id);
        Assert.AreEqual(0, nestedContentPickerOutput.Properties.Count);
    }

    private IOutputExpansionStrategyAccessor CreateOutputExpansionStrategyAccessor(bool expandAll = false, string[]? expandPropertyAliases = null)
    {
        var httpContextMock = new Mock<HttpContext>();
        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var expand = expandAll ? "all" : expandPropertyAliases != null ? $"property:{string.Join(",", expandPropertyAliases)}" : null;
        httpRequestMock
            .SetupGet(r => r.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues> { { "expand", expand } }));

        httpContextMock.SetupGet(c => c.Request).Returns(httpRequestMock.Object);
        httpContextAccessorMock.SetupGet(a => a.HttpContext).Returns(httpContextMock.Object);

        IOutputExpansionStrategy outputExpansionStrategy = new RequestContextOutputExpansionStrategy(httpContextAccessorMock.Object);
        var outputExpansionStrategyAccessorMock = new Mock<IOutputExpansionStrategyAccessor>();
        outputExpansionStrategyAccessorMock.Setup(s => s.TryGetValue(out outputExpansionStrategy)).Returns(true);

        return outputExpansionStrategyAccessorMock.Object;
    }

    private void SetupContentMock(Mock<IPublishedContent> content, params IPublishedProperty[] properties)
    {
        var key = Guid.NewGuid();
        content.SetupGet(c => c.Key).Returns(key);
        content.SetupGet(c => c.UrlSegment).Returns("url-segment");
        content.SetupGet(c => c.Properties).Returns(properties);
        content.SetupGet(c => c.ContentType).Returns(_contentType);
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        RegisterContentWithProviders(content.Object);
    }

    private IPublishedContent CreateSimplePickedContent(int numberOneValue, int numberTwoValue)
    {
        var content = new Mock<IPublishedContent>();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, numberOneValue, "numberOne"),
            CreateNumberProperty(content.Object, numberTwoValue, "numberTwo"));

        return content.Object;
    }

    private IPublishedContent CreateMultiLevelPickedContent(int numberValue, IPublishedContent nestedContentPickerValue, string nestedContentPickerPropertyTypeAlias, ApiContentBuilder apiContentBuilder)
    {
        var content = new Mock<IPublishedContent>();
        SetupContentMock(
            content,
            CreateNumberProperty(content.Object, numberValue, "number"),
            CreateContentPickerProperty(content.Object, nestedContentPickerValue.Key, nestedContentPickerPropertyTypeAlias, apiContentBuilder));

        return content.Object;
    }

    private PublishedElementPropertyBase CreateContentPickerProperty(IPublishedElement parent, Guid pickedContentKey, string propertyTypeAlias, IApiContentBuilder contentBuilder)
    {
        ContentPickerValueConverter contentPickerValueConverter = new ContentPickerValueConverter(PublishedSnapshotAccessor, contentBuilder);
        var contentPickerPropertyType = SetupPublishedPropertyType(contentPickerValueConverter, propertyTypeAlias, Constants.PropertyEditors.Aliases.ContentPicker);

        return new PublishedElementPropertyBase(contentPickerPropertyType, parent, false, PropertyCacheLevel.None, new GuidUdi(Constants.UdiEntityType.Document, pickedContentKey).ToString());
    }

    private PublishedElementPropertyBase CreateNumberProperty(IPublishedElement parent, int propertyValue, string propertyTypeAlias)
    {
        var numberPropertyType = SetupPublishedPropertyType(new IntegerValueConverter(), propertyTypeAlias, Constants.PropertyEditors.Aliases.Label);
        return new PublishedElementPropertyBase(numberPropertyType, parent, false, PropertyCacheLevel.None, propertyValue);
    }

    private PublishedElementPropertyBase CreateElementProperty(
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

        var elementValueConverter = new Mock<IContentApiPropertyValueConverter>();
        elementValueConverter
            .Setup(p => p.ConvertIntermediateToContentApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>()))
            .Returns(() => apiElementBuilder.Build(element.Object));
        elementValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        elementValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        elementValueConverter.Setup(p => p.GetPropertyContentApiCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);

        var elementPropertyType = SetupPublishedPropertyType(elementValueConverter.Object, elementPropertyAlias, "My.Element.Property");
        return new PublishedElementPropertyBase(elementPropertyType, parent, false, PropertyCacheLevel.None);
    }

    private IApiContentRouteBuilder ApiContentRouteBuilder() => new ApiContentRouteBuilder(PublishedUrlProvider, CreateGlobalSettings());
}
