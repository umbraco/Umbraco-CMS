using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Rendering;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

/// <summary>
/// Any tests contained within this class specifically test property expansion V2 (and field limiting) - not V1. If the
/// aim is to test expansion for both versions, please put the tests in the base class.
/// </summary>
[TestFixture]
public class OutputExpansionStrategyV2Tests : OutputExpansionStrategyTestBase
{
    [TestCase("contentPicker", "contentPicker")]
    [TestCase("rootPicker", "nestedPicker")]
    public void OutputExpansionStrategy_CanExpandNestedContentPicker(string rootPropertyTypeAlias, string nestedPropertyTypeAlias)
    {
        var accessor = CreateOutputExpansionStrategyAccessor($"properties[{rootPropertyTypeAlias}[properties[{nestedPropertyTypeAlias}]]]");
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
        Assert.IsNotEmpty(nestedContentPickerOutput.Properties);
        Assert.AreEqual(987, nestedContentPickerOutput.Properties["numberOne"]);
        Assert.AreEqual(654, nestedContentPickerOutput.Properties["numberTwo"]);
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandSpecifiedElement()
    {
        // var accessor = CreateOutputExpansionStrategyAccessor(false, new[] { "element" });
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[$all]]]");
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
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[$all]],element2[properties[$all]]]" );
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
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[contentPicker]]]" );
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

    [Test]
    public void OutputExpansionStrategy_CanExpandElementNestedContentPicker()
    {
        var accessor = CreateOutputExpansionStrategyAccessor("properties[element[properties[contentPicker[properties[nestedContentPicker]]]]]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);
        var apiElementBuilder = new ApiElementBuilder(accessor);

        var nestedContentPickerValue = CreateSimplePickedContent(111, 222);
        var contentPickerValue = CreateMultiLevelPickedContent(987, nestedContentPickerValue, "nestedContentPicker", apiContentBuilder);

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
        var nestedContentPickerOutput = contentPickerOutput.Properties["nestedContentPicker"] as IApiContent;
        Assert.IsNotNull(nestedContentPickerOutput);
        Assert.AreEqual(nestedContentPickerValue.Key, nestedContentPickerOutput.Id);
        Assert.AreEqual(2, nestedContentPickerOutput.Properties.Count);
        Assert.AreEqual(111, nestedContentPickerOutput.Properties["numberOne"]);
        Assert.AreEqual(222, nestedContentPickerOutput.Properties["numberTwo"]);
    }

    [Test]
    public void OutputExpansionStrategy_CanExpandContentPickerBeyondTwoLevels()
    {
        var accessor = CreateOutputExpansionStrategyAccessor($"properties[level1Picker[properties[level2Picker[properties[level3Picker[properties[level4Picker]]]]]]]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = new Mock<IPublishedContent>();

        var level5PickedContent = CreateSimplePickedContent(1234, 5678);
        var level4PickedContent = CreateMultiLevelPickedContent(444, level5PickedContent, "level4Picker", apiContentBuilder);
        var level3PickedContent = CreateMultiLevelPickedContent(333, level4PickedContent, "level3Picker", apiContentBuilder);
        var level2PickedContent = CreateMultiLevelPickedContent(222, level3PickedContent, "level2Picker", apiContentBuilder);
        var contentPickerContentProperty = CreateContentPickerProperty(content.Object, level2PickedContent.Key, "level1Picker", apiContentBuilder);

        SetupContentMock(content, contentPickerContentProperty);

        var result = apiContentBuilder.Build(content.Object);

        Assert.AreEqual(1, result.Properties.Count);

        var level1PickerOutput = result.Properties["level1Picker"] as ApiContent;
        Assert.IsNotNull(level1PickerOutput);
        Assert.AreEqual(level2PickedContent.Key, level1PickerOutput.Id);
        Assert.AreEqual(2, level1PickerOutput.Properties.Count);
        Assert.AreEqual(222, level1PickerOutput.Properties["number"]);

        var level2PickerOutput = level1PickerOutput.Properties["level2Picker"] as ApiContent;
        Assert.IsNotNull(level2PickerOutput);
        Assert.AreEqual(level3PickedContent.Key, level2PickerOutput.Id);
        Assert.AreEqual(2, level2PickerOutput.Properties.Count);
        Assert.AreEqual(333, level2PickerOutput.Properties["number"]);

        var level3PickerOutput = level2PickerOutput.Properties["level3Picker"] as ApiContent;
        Assert.IsNotNull(level3PickerOutput);
        Assert.AreEqual(level4PickedContent.Key, level3PickerOutput.Id);
        Assert.AreEqual(2, level3PickerOutput.Properties.Count);
        Assert.AreEqual(444, level3PickerOutput.Properties["number"]);

        var level4PickerOutput = level3PickerOutput.Properties["level4Picker"] as ApiContent;
        Assert.IsNotNull(level4PickerOutput);
        Assert.AreEqual(level5PickedContent.Key, level4PickerOutput.Id);
        Assert.AreEqual(2, level4PickerOutput.Properties.Count);
        Assert.AreEqual(1234, level4PickerOutput.Properties["numberOne"]);
        Assert.AreEqual(5678, level4PickerOutput.Properties["numberTwo"]);
    }

    [TestCase("numberOne")]
    [TestCase("numberTwo")]
    public void OutputExpansionStrategy_CanLimitDirectFields(string includedField)
    {
        var accessor = CreateOutputExpansionStrategyAccessor(fields: $"properties[{includedField}]");
        var apiContentBuilder = new ApiContentBuilder(new ApiContentNameProvider(), ApiContentRouteBuilder(), accessor);

        var content = CreateSimplePickedContent(123, 456);

        var result = apiContentBuilder.Build(content);

        Assert.AreEqual(1, result.Properties.Count);
        Assert.IsTrue(result.Properties.ContainsKey(includedField));
        Assert.AreEqual(includedField is "numberOne" ? 123 : 456, result.Properties[includedField]);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OutputExpansionStrategy_CanLimitFieldsOfExpandedContent(bool expand)
    {
        var accessor = CreateOutputExpansionStrategyAccessor(expand ? "properties[$all]" : null, "properties[contentPickerOne[properties[numberOne]],contentPickerTwo[properties[numberTwo]]]");
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
        // yeah we shouldn't test two things in one unit test, but given the risk of false positives when testing
        // conditional field limiting, this is preferable.
        if (expand)
        {
            Assert.AreEqual(1, contentPickerOneOutput.Properties.Count);
            Assert.AreEqual(12, contentPickerOneOutput.Properties["numberOne"]);
        }
        else
        {
            Assert.IsEmpty(contentPickerOneOutput.Properties);
        }

        var contentPickerTwoOutput = result.Properties["contentPickerTwo"] as ApiContent;
        Assert.IsNotNull(contentPickerTwoOutput);
        Assert.AreEqual(contentPickerTwoContent.Key, contentPickerTwoOutput.Id);
        if (expand)
        {
            Assert.AreEqual(1, contentPickerTwoOutput.Properties.Count);
            Assert.AreEqual(78, contentPickerTwoOutput.Properties["numberTwo"]);
        }
        else
        {
            Assert.IsEmpty(contentPickerTwoOutput.Properties);
        }
    }

    protected override IOutputExpansionStrategyAccessor CreateOutputExpansionStrategyAccessor(string? expand = null, string? fields = null)
    {
        var httpContextMock = new Mock<HttpContext>();
        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        httpRequestMock
            .SetupGet(r => r.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues> { { "expand", expand }, { "fields", fields } }));

        httpContextMock.SetupGet(c => c.Request).Returns(httpRequestMock.Object);
        httpContextAccessorMock.SetupGet(a => a.HttpContext).Returns(httpContextMock.Object);
        IOutputExpansionStrategy outputExpansionStrategy = new RequestContextOutputExpansionStrategyV2(
            httpContextAccessorMock.Object,
            new ApiPropertyRenderer(new NoopPublishedValueFallback()),
            Mock.Of<ILogger<RequestContextOutputExpansionStrategyV2>>());
        var outputExpansionStrategyAccessorMock = new Mock<IOutputExpansionStrategyAccessor>();
        outputExpansionStrategyAccessorMock.Setup(s => s.TryGetValue(out outputExpansionStrategy)).Returns(true);

        return outputExpansionStrategyAccessorMock.Object;
    }

    protected override string? FormatExpandSyntax(bool expandAll = false, string[]? expandPropertyAliases = null)
        => expandAll ? "$all" : expandPropertyAliases?.Any() is true ? $"properties[{string.Join(",", expandPropertyAliases)}]" : null;
}
